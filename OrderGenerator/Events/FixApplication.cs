using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX44;
using QuickFix.Logger;
using QuickFix.Store;

public class FixApplication : MessageCracker, IApplication
{
    private SessionSettings _settings;
    private IMessageStoreFactory _storeFactory;
    private ILogFactory _logFactory;
    private QuickFix.Transport.SocketInitiator _initiator;

    private Dictionary<SessionID, Session> _activeSessions = new Dictionary<SessionID, Session>();

    public bool IsConnected { get; internal set; }

    public event Action<string> OnExecutionReportReceived;


    public FixApplication()
    {
        IsConnected = false;
        _settings = new SessionSettings("initiator.config");
        _storeFactory = new FileStoreFactory(_settings);
        _logFactory = new FileLogFactory(_settings);
        _initiator = new QuickFix.Transport.SocketInitiator(this, _storeFactory, _settings, _logFactory);
    }

    public void FromAdmin(QuickFix.Message message, SessionID sessionID)
    {
       // Console.WriteLine("FromAdmin");
    }

    public void FromApp(QuickFix.Message message, SessionID sessionID)
    {
        //Console.WriteLine("FromApp");
        //Console.WriteLine("Received ExecutionReport: " + message);
        ExecutionReport executionReportData = (ExecutionReport)message;
        OrdersController.OnExecutionReport(executionReportData);

        /*
        try
        {
            Crack(message, sessionID);  // This is crucial to direct messages to the OnMessage handlers
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception in FromApp: " + e.ToString());
        }*/
    }

    public void OnCreate(SessionID sessionID)
    {
        //Console.WriteLine("OnCreate");
    }

    public void OnLogon(SessionID sessionID)
    {

        Console.WriteLine("Logged");
        IsConnected = true;

        if (!_activeSessions.ContainsKey(sessionID))
        {
            Console.WriteLine("New session");
            _activeSessions[sessionID] = Session.LookupSession(sessionID);
        }
        else
        {
            Console.WriteLine("Existing session");
        }
    }

    public void OnLogout(SessionID sessionID)
    {
        IsConnected = false;
        _activeSessions.Remove(sessionID);
    }

    public void Start()
    {
        _initiator.Start();
    }

    public void Stop()
    {
        _initiator.Stop();
    }

    public void ToAdmin(QuickFix.Message message, SessionID sessionID)
    {
        //Console.WriteLine("ToAdmin"); 
    }

    public void ToApp(QuickFix.Message message, SessionID sessionID)
    {
        //Console.WriteLine("ToApp");
    }

    internal void Send(QuickFix.Message message)
    {
        if (_activeSessions.Count > 0)
        {
            var firstActiveSession = _activeSessions.Keys.First();
            try
            {
                Session.SendToTarget(message, firstActiveSession);
            }
            catch (SessionNotFound ex)
            {
                Console.WriteLine($"Failed to send message: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("No active FIX sessions available.");
        }
    }
    /*
    public void OnMessage(QuickFix.Message message, SessionID sessionID)
    {
        Console.WriteLine("Received NewOrderSingle: " + message);
        ExecutionReport executionReportData = (ExecutionReport)message;
        OrdersController.OnExecutionReport(executionReportData);
    }
    */

}
