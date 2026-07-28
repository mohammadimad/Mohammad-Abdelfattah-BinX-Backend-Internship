using System.Diagnostics;

namespace Day03
{
    //public class ProductRepository
    //{
    //    public async Task<List<string>> GetProductsFromDbAsync()
    //    {
    //        await Task.Delay(1000);
    //        return new List<string> { "Laptop", "Mouse", "Keyboard" };
    //    }
    //}
    //public class ProductService {
    //    private  readonly ProductRepository _repository = new ProductRepository();
    //    public ProductService(ProductRepository productRepository) {
    //        _repository = productRepository;
    //    }
    //    public  async Task<List<string>> GetAllProductsAsync()
    //    {
    //        var products = await _repository.GetProductsFromDbAsync();

    //        return products.Select(p=> p.ToUpper()).ToList();
    //    }
    //}
    //public class ProductsController {
    //    private readonly ProductService _service;
    //    public ProductsController(ProductService service)
    //    {
    //        _service = service;
    //    }
    //    public async Task<string> GetAsync()
    //    {
    //        var result = await _service.GetAllProductsAsync();
    //        return $"[200oK]Data Retrieved: {string.Join(", ",result)}";
    //    }

    //}
    public class ExternalDataService
    {
        //task1
        public async Task<List<string>> GetWeatherDataAsync()
        {
            await Task.Delay(1000);
            return new List<string> { "Hot","Cold" };
        }
        public async Task<List<decimal>> GetCurrencyRatesAsync()
        {
            await Task.Delay(2000);
            return new List<decimal> { 1.4m, 2.5m };
        }
        public async Task<List<string>> GetStockInventoryAsync(CancellationToken ct)
        {
            await Task.Delay(1000,ct);
            return new List<string> { "rice", "meet", "egg" };
        }
    }
    internal class Program
    {

        static async  Task Main(string[] args)
        {
            //task 2
            ExternalDataService dataService = new ExternalDataService();

            Console.WriteLine("[Sequential Test Started]");
            var stopwatch = Stopwatch.StartNew();
            var cts = new CancellationTokenSource();
            var r1 = await dataService.GetWeatherDataAsync();
           var r2 = await dataService.GetCurrencyRatesAsync();
           var r3 = await dataService.GetStockInventoryAsync(cts.Token);
           
            stopwatch.Stop();
            Console.WriteLine($"⏱️ Sequential Total Elapsed Time: {stopwatch.ElapsedMilliseconds}");
            //task3
            stopwatch.Restart();
            Console.WriteLine("After use WhenAll");
            Task t1 = dataService.GetWeatherDataAsync();
            Task t2 = dataService.GetCurrencyRatesAsync();
            Task t3 = dataService.GetStockInventoryAsync(cts.Token);
            await Task.WhenAll(t1, t2, t3);
            stopwatch.Stop();
            Console.WriteLine($"⏱️ Sequential Total Elapsed Time: {stopwatch.ElapsedMilliseconds}");

            //task4
            using (var ct = new CancellationTokenSource())
            {
                ct.CancelAfter(500);

                try
                {

                    List<string> data = await dataService.GetStockInventoryAsync(cts.Token);

                    Console.WriteLine($"-> Data Received: {string.Join(", ", data)}");
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("\n❌ Operation was canceled by the CancellationToken!");
                }
            }

        }
     
    }
}
