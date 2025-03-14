public class Order
{
    public string Symbol { get; set; }
    public int Quantity { get; set; }
    public double Price { get; set; }
    public char Side { get; set; } // '1' for Buy, '2' for Sell
}
