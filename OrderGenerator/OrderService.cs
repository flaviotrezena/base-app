using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX44;

public class OrderService
{
    private readonly FixApplication _fixApplication;
    private TaskCompletionSource<string> _executionReportTask;

    public OrderService(FixApplication fixApplication)
    {
        _fixApplication = fixApplication;
        _fixApplication.OnExecutionReportReceived += HandleExecutionReport;
    }

    private void HandleExecutionReport(string executionReport)
    {
        Console.WriteLine("Execution report received by service.");
        _executionReportTask?.SetResult(executionReport);
    }

    public async Task<string> SendOrderAndWaitForExecutionReport()
    {
        _executionReportTask = new TaskCompletionSource<string>();

        Console.WriteLine("Starting FIX Client...");
        Task.Run(() => _fixApplication.Start());

        return await _executionReportTask.Task; // Wait for ExecutionReport to be received
    }

    public static string GenerateClOrderId()
    {
        return $"{DateTime.UtcNow:yyyyMMddHHmmssfff}{new Random().Next(1000, 9999)}";
    }

    public void SendOrder(Order order)
    {

        var newOrderSingle = new NewOrderSingle();
        newOrderSingle.Set(new ClOrdID(GenerateClOrderId()));
        newOrderSingle.Set(new Side(ConvertToFixSide(order.Side)));
        newOrderSingle.Set(new TransactTime(DateTime.UtcNow));
        newOrderSingle.Set(new OrdType(OrdType.LIMIT));
        newOrderSingle.Set(new Symbol(order.Symbol));
        newOrderSingle.Set(new OrderQty(order.Quantity));
        newOrderSingle.Set(new Price((decimal)order.Price));

        // Send the order to the FIX session
        _fixApplication.Send(newOrderSingle);

    }

    private char ConvertToFixSide(char side)
    {
        return side == '1' ? Side.BUY : Side.SELL;
    }

}
