using QuickFix;
using QuickFix.Logger;
using QuickFix.Store;

namespace OrderAccumulator
{
    class Program
    {
        static void Main(string[] args)
        {
            SessionSettings settings = new SessionSettings("acceptor.cfg");
            IApplication application = new FixApplication();
            IMessageStoreFactory storeFactory = new FileStoreFactory(settings);
            ILogFactory logFactory = new FileLogFactory(settings);
            IAcceptor acceptor = new ThreadedSocketAcceptor(application, storeFactory, settings, logFactory);

            acceptor.Start();
            Console.WriteLine("Press <ENTER> to quit");
            Console.ReadLine();

            acceptor.Stop();
        }
    }

    
}
