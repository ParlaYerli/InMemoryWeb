using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InMemoryWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace InMemoryWeb.Controllers
{
    public class ProductController : Controller
    {
        private IMemoryCache _memoryCache;
        public ProductController(IMemoryCache _memoryCache)
        {
            this._memoryCache = _memoryCache;
        }
      
        public IActionResult Index()
        {
          /*  //1.yol
            if (String.IsNullOrEmpty(_memoryCache.Get<string>("zaman"))) // böyle bir key yoksa oluşturacak
            {
                 _memoryCache.Set<string>("zaman", DateTime.Now.ToString());
            }
            */
            //2.yol
            if (!_memoryCache.TryGetValue("zaman",out string zamancache))
            {
                MemoryCacheEntryOptions options = new MemoryCacheEntryOptions();
                options.AbsoluteExpiration = DateTime.Now.AddSeconds(10);// cachein ömrünü belirler.Sliding ile kullanılmasının sebebi var olan cachei öldürüp güncel bilginin cachete tutulmasını sağlmaktır.

               // options.SlidingExpiration = TimeSpan.FromSeconds(10); // belirlenen saniye boyunca istek gelirse o kadar saniye daha cache ömrü uzatılır.
                options.Priority = CacheItemPriority.High; //cache belleği dolunca cachein sırayla silinmesi için öncelik belirleme olayı gerçekleştirilir.
               
                //memoryden data silinince hangi sebepten silindi ise onu öğrenmemizi sağlar.
                options.RegisterPostEvictionCallback((key, value, reason, state) => {
                    _memoryCache.Set("callback", $"{key} -> {value} => sebep: {reason}");
                });
                _memoryCache.Set<string>("zaman", DateTime.Now.ToString(),options);
                Product p = new Product { Id = 1, Name = "kalem", Prica = 20 };
                _memoryCache.Set<Product>("product:1", p);
            }
            
            return View();
        }

        public IActionResult Show()
        {
            /*  //getorcreate varsa getirir yoksa oluşturur.
              _memoryCache.GetOrCreate<string>("zaman", entry => 
              {
                  return DateTime.Now.ToString();
              });*/
            _memoryCache.TryGetValue("zaman", out string zamancache); // zaman adında keyin value değerini zamancache'e atayacak.
            _memoryCache.TryGetValue("callback", out string callback);
            ViewBag.product = _memoryCache.Get<Product>("product:1");
            ViewBag.zaman = zamancache;
            ViewBag.callback = callback;
            return View();
        }
    }
}