using Currency;
using System.Net;
using System.Net.Sockets;
using System.Xml.Serialization;

public class Server
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Server started");
        await WaitClientQuery();
        Console.ReadKey();
    }

    public static async Task WaitClientQuery()
    {
        await Task.Run(async () =>
        {
            try
            {
                using UdpClient client = new UdpClient(2077);

                while (true)
                {
                    UdpReceiveResult res = await client.ReceiveAsync();
                    IPEndPoint remote = res.RemoteEndPoint;
                    byte[] arr = res.Buffer;

                    if (arr.Length > 0)
                    {
                        using (MemoryStream stream = new MemoryStream(arr))
                        {
                            XmlSerializer clSerializer = new XmlSerializer(typeof(CurrencyClient));
                            CurrencyClient cl = clSerializer.Deserialize(stream) as CurrencyClient;

                            if (cl.user.Contains("connected"))
                            {
                                Console.WriteLine($"[{DateTime.Now}] {cl.user} to the server");
                            }
                            else if (cl.user.Contains("left"))
                            {
                                Console.WriteLine($"[{DateTime.Now}] {cl.user} the server");
                            }
                            else
                            {
                                Console.WriteLine($"[{DateTime.Now}] {cl.user} requested exchange: {cl.c1} to {cl.c2}");

                                var s = new CurrencyServer();

                                if (ExchangeRates.TryGetValue((cl.c1.Value, cl.c2.Value), out decimal rate))
                                {
                                    s.res = rate.ToString();
                                    using (MemoryStream responseStream = new MemoryStream())
                                    {
                                        XmlSerializer servSerializer = new XmlSerializer(typeof(CurrencyServer));
                                        servSerializer.Serialize(responseStream, s);
                                        byte[] responseArr = responseStream.ToArray();
                                        await client.SendAsync(responseArr, responseArr.Length, remote);
                                    }

                                    Console.WriteLine($"[{DateTime.Now}] Sent rate: {rate} for {cl.c1}->{cl.c2}");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[SERVER ERROR] " + ex.Message);
            }
        });
    }

    private static readonly Dictionary<(CurrencyName, CurrencyName), decimal> ExchangeRates =
        new Dictionary<(CurrencyName, CurrencyName), decimal>
    {
        { (CurrencyName.UAH, CurrencyName.USD), 0.0274m },
        { (CurrencyName.UAH, CurrencyName.EUR), 0.0233m },
        { (CurrencyName.UAH, CurrencyName.PLN), 0.1087m },

        { (CurrencyName.USD, CurrencyName.UAH), 36.50m },
        { (CurrencyName.USD, CurrencyName.EUR), 0.85m },
        { (CurrencyName.USD, CurrencyName.PLN), 3.95m },

        { (CurrencyName.EUR, CurrencyName.UAH), 42.94m },
        { (CurrencyName.EUR, CurrencyName.USD), 1.1765m },
        { (CurrencyName.EUR, CurrencyName.PLN), 4.65m },

        { (CurrencyName.PLN, CurrencyName.UAH), 9.20m },
        { (CurrencyName.PLN, CurrencyName.USD), 0.2532m },
        { (CurrencyName.PLN, CurrencyName.EUR), 0.2151m }
    };
}