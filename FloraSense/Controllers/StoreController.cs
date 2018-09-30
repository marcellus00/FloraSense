using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Services.Store;

namespace FloraSense
{
    public class StoreController
    {
        public const string FloraSenseAdFree = "9NGBX22P9BVH";

        public bool FloraSenseAdFreePurchased => _userCollection?.ContainsKey(FloraSenseAdFree) ?? false;

        public StoreContext StoreContext { get; private set; }
        public bool InitialCheck { get; private set; }

        private IReadOnlyDictionary<string, StoreProduct> _userCollection;
        private readonly List<string> _productKinds = new List<string> { "Durable" };
        
        public async Task UpdatePurchasesInfo()
        {
            if (StoreContext == null)
                StoreContext = StoreContext.GetDefault();

            var queryResult = await StoreContext.GetUserCollectionAsync(_productKinds);
            _userCollection = queryResult.Products;
            InitialCheck = true;
        }
    }
}
