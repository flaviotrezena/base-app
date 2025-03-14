using OrderAccumulator.Model;
using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX44;

namespace OrderAccumulator
{
    public class FixApplication : MessageCracker, IApplication
    {
        static private List<Order> orderList = new List<Order>();

        public void ToAdmin(QuickFix.Message message, SessionID sessionID)
        {
         //   Console.WriteLine("ToAdmin");
        }


        public void OnMessage(NewOrderSingle message, SessionID sessionID)
        {   
            Order newOrder = ConverteMensagem(message);

            bool isOverLimit = ProcessMessage(newOrder);
            if (!isOverLimit)
            {
                SendExecutionReport(sessionID, message, ExecType.NEW, OrdStatus.NEW);
            }
            else
            {
                SendExecutionReport(sessionID, message, ExecType.REJECTED, OrdStatus.REJECTED);
            }
        }

        private Order ConverteMensagem(NewOrderSingle newOrderSingleMessage)
        {
            Order order = new Order();
            order.Price = newOrderSingleMessage.Price.Value;
            order.Symbol = newOrderSingleMessage.Symbol.Value;
            order.Side = (newOrderSingleMessage.Side.Value.ToString() == "1") ? 'C' : 'V';
            order.Quantity = newOrderSingleMessage.OrderQty.Value;

            return order;
        }

    
        private bool ProcessMessage(Order order)
        {

            //calcula exposicao para a nova ordem 
            bool isExposed = FinanceExposure.Check(orderList, order.Symbol);

            Console.WriteLine($"Check Before for Ticker [{order.Symbol}] is exposed ? [{isExposed}]");  

            if (!isExposed)
            {
                //adiciono a ordem a lista de ordens
                orderList.Add(order);
                Console.WriteLine($"Received Ticker={order.Symbol} Side={order.Side} PU={order.Price} Qty={order.Quantity}");
                isExposed = FinanceExposure.Check(orderList, order.Symbol);
            }

            Console.WriteLine($"Check After for Ticker [{order.Symbol}] is exposed ? [{isExposed}]");

            return isExposed;
        }

        public void FromAdmin(QuickFix.Message message, SessionID sessionID)
        {
            //Console.WriteLine("FromAdmin");

        }

        public void ToApp(QuickFix.Message message, SessionID sessionID)
        {
            //Console.WriteLine("ToApp");
        }

        public void FromApp(QuickFix.Message message, SessionID sessionID)
        {
            //Console.WriteLine("FromApp");
            try
            {
                Crack(message, sessionID);  // This is crucial to direct messages to the OnMessage handlers
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception in FromApp: " + e.ToString());
            }
        }

        public void OnCreate(SessionID sessionID)
        {
            //Console.WriteLine("OnCreate");
        }

        public void OnLogout(SessionID sessionID)
        {
            //Console.WriteLine("OnLogout");
        }

        public void OnLogon(SessionID sessionID)
        {
            Console.WriteLine("Logged");
        }


        private void SendExecutionReport(SessionID sessionID, NewOrderSingle order, char execType, char ordStatus)
        {
            var execReport = new ExecutionReport();

            execReport.Set(new OrderID(order.ClOrdID.Value));
            execReport.Set(new ExecID(System.Guid.NewGuid().ToString()));
            execReport.Set(new ExecType(execType));
            execReport.Set(new OrdStatus(ordStatus));
            execReport.Set(new Side(order.Side.Value));
            execReport.Set(new LeavesQty(0));
            execReport.Set(new CumQty(0));
            execReport.Set(new AvgPx(0));
            execReport.Set(new Symbol(order.Symbol.Value));
            execReport.Set(new LastPx(0)); 

            try
            {
                Session.SendToTarget(execReport, sessionID);
                Console.WriteLine("Execution Report sent.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send Execution Report: {ex.Message}");
            }
        }
    }
}
