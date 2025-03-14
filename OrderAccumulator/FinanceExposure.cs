
using OrderAccumulator.Model;

namespace OrderAccumulator
{
    internal class FinanceExposure
    {
        private const decimal LIMITE = 1000m;

        internal static bool Check(List<Model.Order> ordersRecebidas, String newOrder)
        {
            var papeis = new[] { newOrder };

            bool ehExposto = false;

            foreach (var papel in papeis)
            {
                Console.WriteLine($"Validating exposure for {papel}");

                decimal valCompra = ordersRecebidas
                    .Where(o => o.Symbol == papel && o.Side == 'C')
                    .Sum(o => o.Price * o.Quantity);

                Console.WriteLine($"Total Side buy: {valCompra}");

                decimal valVenda = ordersRecebidas
                    .Where(o => o.Symbol == papel && o.Side == 'V')
                    .Sum(o => o.Price * o.Quantity);

                Console.WriteLine($"Total Side sell: {valCompra}");

                decimal exposicao = valCompra - valVenda;

                Console.WriteLine("***************************");

                if (exposicao >= LIMITE)
                    return true;
            }

            return ehExposto;
        }
    }
}