namespace Currency
{
    [Serializable]
    public enum CurrencyName { UAH, USD, EUR, PLN }
    [Serializable]
    public class CurrencyClient
    {
        public CurrencyName? c1;
        public CurrencyName? c2;
        public string user;
    }
    [Serializable]
    public class CurrencyServer
    {
        public string res;
    }
}
