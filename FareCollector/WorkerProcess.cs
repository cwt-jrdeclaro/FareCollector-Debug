using DocumentManager;
using SabreFareRulesInterface.MDFSabreAirTicketService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using FareCollector.Exceptions;
using SabreFareRulesInterface.Object;
using System.Configuration;

namespace FareCollector
{
    public class WorkerProcess
    {
        


        private static MSMQCommunication.Manager manager = new MSMQCommunication.Manager(AppSettings.InboundQueue, AppSettings.HP_InboundQueue, Utility.AppCodes.FARE_COLLECTOR);
        SabreFareRulesInterface.Manager interfaceManager;

        MessageQueueTransaction inTransaction = new MessageQueueTransaction();
        MessageQueueTransaction outTransaction = new MessageQueueTransaction();
        TravelDocument doc = new TravelDocument();
        MessagePriority currentMessagePriority = MessagePriority.Normal;
        string result = string.Empty;
        FareRulesReq docBankRequest;
        Message msg;
        private SabreAirTicketServicePortChannel channel = null;

        public WorkerProcess(SabreAirTicketServicePortChannel _channel)
        {
            channel = _channel;           
        }
        public void Start(object state = null)
        {
#if DEBUG
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
#endif
            
#if DEBUG
            Console.WriteLine(watch.Elapsed.ToString());
            General._ActivityLogger.WriteLogEntry(Thread.CurrentThread.Name + ": Initialization complete " + watch.Elapsed.ToString());
#endif
            try
            {
                General._ApplicationLogger.Debug(Thread.CurrentThread.Name + " has begun processing", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString(), System.Reflection.MethodInfo.GetCurrentMethod().Name);
                try
                {
                    msg = RetrieveNextMessage(ref inTransaction, ref currentMessagePriority);
                }
                catch (Exceptions.NoMessageAvailableException ex)
                {
                    return;
                }


#if DEBUG
                General._ActivityLogger.WriteLogEntry(Thread.CurrentThread.Name + ": Message Retrieved " + watch.Elapsed.ToString());
#endif
                doc = ReadMapRecievedMessage(msg, ref inTransaction);

                



                interfaceManager = new SabreFareRulesInterface.Manager((int)Utility.AppCodes.FARE_COLLECTOR, ref channel);
#if DEBUG
                General._ActivityLogger.WriteLogEntry(Thread.CurrentThread.Name + ": Message Read " + watch.Elapsed.ToString());
#endif
                docBankRequest = MapTravelDocToFareRequest(doc);

#if DEBUG
                General._ActivityLogger.WriteLogEntry(Thread.CurrentThread.Name + ": Message Mapped To Object " + watch.Elapsed.ToString());
#endif
                result = GetFareRules(docBankRequest);
#if DEBUG
                General._ActivityLogger.WriteLogEntry(Thread.CurrentThread.Name + ": Fare Rules Retrieved " + watch.Elapsed.ToString());
#endif
                SendFareToManager(result);
            }
            catch (Exception ex)
            {
                General._ApplicationLogger.Error(Thread.CurrentThread.Name + " had an error processing ticket " + doc.ValidatingCarrierNumber + doc.DocumentNo + " " + ex.Message, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString(), System.Reflection.MethodInfo.GetCurrentMethod().Name);
            }

            if (outTransaction.Status == MessageQueueTransactionStatus.Pending)
            {
                outTransaction.Commit();
            }
            if (inTransaction.Status == MessageQueueTransactionStatus.Pending)
            {
                inTransaction.Commit();
            }
#if DEBUG
            General._ActivityLogger.WriteLogEntry(Thread.CurrentThread.Name + ": Message Sent To Manager " + watch.Elapsed.ToString());
#endif
        }

        private void SendFareToManager(string result)
        {
            try
            {
                MSMQCommunication.Manager outbound = new MSMQCommunication.Manager(AppSettings.OutboundQueue, AppSettings.HP_OutboundQueue, Utility.AppCodes.FARE_COLLECTOR);
                SabreFareRulesInterface.Sabre.StructureFareRulesRS FareRulesRS;
                SabreFareRulesInterface.Object.FareRuleResponse response;
                try
                {
                    FareRulesRS = General.DeserializeXMLToSabreObject<SabreFareRulesInterface.Sabre.StructureFareRulesRS>(result);
                    response = new SabreFareRulesInterface.Object.FareRuleResponse(FareRulesRS, result, doc);
                }
                catch (Exception ex)
                {
                    //Document is an MDF ERROR
                    response = new SabreFareRulesInterface.Object.FareRuleResponse();
                    response.ResponseString = result;
                    response.ResponseStatusCode = Utility.FareRulesStatusCode.MDF_ERROR;
                }                

                response.DocumentNumber = doc.DocumentNo;
                response.ValidatingCarrier = doc.ValidatingCarrierNumber;
                response.TransactionID = docBankRequest.TransactionID;

                outbound.SendMessage(Utility.General.SerlizeObject<SabreFareRulesInterface.Object.FareRuleResponse>(response), currentMessagePriority, ref outTransaction);
            }
            catch (Exception ex)
            {
                inTransaction.Abort();
                try
                {
                    outTransaction.Abort();
                }
                catch
                {
                    //Do nothing out transaction has not occured yet
                }
                throw new UnableToSendToManagerExeption("Failed To send Fare Rules For " + doc.ValidatingCarrierNumber.ToString() + " " + doc.DocumentNo.ToString() + ex.Message, ex.InnerException);
            }
        }

        private string GetFareRules(FareRulesReq docBankRequest)
        {
            try
            {
                return interfaceManager.GetFareRuleXML(docBankRequest);
            }
            catch (Oracle.ManagedDataAccess.Client.OracleException oEx)
            {
                ThreadManager.currentTimoutErrorCount++;
                inTransaction.Abort();
                General._ApplicationLogger.Error(Thread.CurrentThread.Name + " had an error communicating with the Database " + oEx.Message + " " + oEx.StackTrace.ToString(), System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString(), System.Reflection.MethodInfo.GetCurrentMethod().Name);
                throw oEx;
            }
            catch (TimeoutException tEx)
            {
                ThreadManager.currentTimoutErrorCount++;
                inTransaction.Abort();
                throw tEx;
            }
            catch (Exception ex)
            {
                if (ex is System.ServiceModel.EndpointNotFoundException
                    || ex is System.ServiceModel.CommunicationException
                    || ex is System.Net.WebException
                    || ex is System.ServiceModel.ServerTooBusyException)
                {
                    inTransaction.Abort();
                    ThreadManager.currentTimoutErrorCount++;                    
                    throw ex;
                }
                else
                {
                    //Remove this ticket. It will always fail
                    try
                    {
                        inTransaction.Commit();
                    }
                    catch (InvalidOperationException e)
                    {
                        //Do Nothing Transaction never started
                    }
                    
                    General._FailedMessageLogger.Error("Unable to retrieve fare rules " + ex.Message + " : " + msg.Body);
                    throw ex;
                }
            }
        }

        private FareRulesReq MapTravelDocToFareRequest(TravelDocument doc)
        {
            try
            {
                return SabreFareRulesInterface.General.MapTravelDocumentCouponsToFareRuleReq(doc, doc.Coupons);
            }
            catch (Oracle.ManagedDataAccess.Client.OracleException oEx)
            {
                ThreadManager.currentTimoutErrorCount++;
                inTransaction.Abort();
                General._ApplicationLogger.Debug(Thread.CurrentThread.Name + " had an error mapping document bank record to Fare Request " + oEx.Message + " " + oEx.StackTrace.ToString(), System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString(), System.Reflection.MethodInfo.GetCurrentMethod().Name);
                throw oEx;
            }
            catch (Exception ex)
            {
                General._ApplicationLogger.Error(Thread.CurrentThread.Name + " had an error formatting " + doc.ValidatingCarrierNumber + doc.DocumentNo + " into a fare rules request " + ex.Message + " " + ex.StackTrace.ToString());
                General._FailedMessageLogger.Error("PROBLEM MAPPING TO FARE REQUEST :" + msg.Body);
                inTransaction.Commit();
                throw new UnableToReadMessageException("Unable to map Travel_document to request - " + ex.Message, msg.Body.ToString(), ex.InnerException);
            }

        }


        private static TravelDocument ReadMapRecievedMessage(Message msg, ref MessageQueueTransaction inTransaction)
        {
            TravelDocument doc = new TravelDocument();
            try
            {
                msg.Formatter = new XmlMessageFormatter(new string[] { "System.String,mscorlib" });
                doc = Utility.General.DeserializeObject<TravelDocument>(msg.Body.ToString());
            }
            catch (Exception ex)
            {
                inTransaction.Commit();
                General._ApplicationLogger.Error(Thread.CurrentThread.Name + " - INVALID MESSAGE RECIEVED FROM: " + AppSettings.InboundQueue + " " + ex.Message);
                General._FailedMessageLogger.Error("PROBLEM DESERIALIZING TRAVEL DOCUMENT MESSAGE: " + msg.Body);
                throw new UnableToReadMessageException("Error deserializing travel_document message - " + ex.Message, msg.Body.ToString(), ex.InnerException);
            }
            return doc;
        }

        private static Message RetrieveNextMessage(ref MessageQueueTransaction inTransaction, ref MessagePriority currentMessagePriority)
        {
            Message msg = new Message();
            inTransaction = new MessageQueueTransaction();

            //Try and get HP Message First
            try
            {
                msg = manager.RetrieveNextMessage(ref inTransaction, MessagePriority.High);
                currentMessagePriority = MessagePriority.High;
            }
            catch (MessageQueueException ex)
            {
                if (ex.MessageQueueErrorCode == MessageQueueErrorCode.IOTimeout)
                {
                    //Still need to check regualar
                }
                else
                {
                    General._ApplicationLogger.Error(Thread.CurrentThread.Name + " Error attempting to communicate with message QUEUE " + AppSettings.HP_InboundQueue + " " + ex.Message);
                    throw ex;
                }
            }
            //Try and get Normal Priority Message if HP not available
            if (string.IsNullOrEmpty(msg.TransactionId))
            {
                try
                {
                    inTransaction = new MessageQueueTransaction();
                    msg = manager.RetrieveNextMessage(ref inTransaction, MessagePriority.Normal);
                    currentMessagePriority = MessagePriority.Normal;
                }
                catch (MessageQueueException ex)
                {
                    if (ex.MessageQueueErrorCode == MessageQueueErrorCode.IOTimeout)
                    {
                        throw new Exceptions.NoMessageAvailableException("No Messages in queues");
                    }
                    else
                    {
                        General._ApplicationLogger.Error(Thread.CurrentThread.Name + " Error attempting to communicate with message QUEUE " + AppSettings.InboundQueue + " " + ex.Message);
                        throw ex;
                    }
                }
            }
            return msg;
        }




    }
}
