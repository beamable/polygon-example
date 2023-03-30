using System;
using System.Collections.Generic;
using System.Text;
using Beamable;
using Beamable.Common.Api.Inventory;
using Beamable.Common.Inventory;
using Beamable.UI.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace PolygonExamples.Scripts
{
    /// <summary>
    /// A script that presents how to perform operations related to federated inventory items
    /// </summary>
    public class InventoryPage : TabPage
    {
        [SerializeField] private Button _getInventoryButton;

        [SerializeField] private ItemPresenter _itemPresenter;
        [SerializeField] private Transform _itemsParent;

        private readonly Dictionary<string, Sprite> _cachedSprites = new();

        private void Start()
        {
            _getInventoryButton.onClick.AddListener(OnGetInventoryClicked);
            DownloadSprites();
        }

        private async void DownloadSprites()
        {
            Data.Working = true;

            try
            {
                CurrencyContent currencyContent = await Data.CurrencyRef.Resolve();
                currencyContent.icon.LoadAssetAsync<Sprite>().Completed += handle =>
                {
                    _cachedSprites.Add(currencyContent.Id, handle.Result);

                    if (_cachedSprites.Count == 2)
                    {
                        Data.Working = false;
                    }
                };
            }
            catch (Exception)
            {
                OnLog("<color=#FF0000>Create federated currency content in ContentManager " +
                      $"and set a reference in {nameof(PolygonAuthExample)}</color>");
            }

            try
            {
                ItemContent itemContent = await Data.ItemRef.Resolve();
                itemContent.icon.LoadAssetAsync<Sprite>().Completed += handle =>
                {
                    _cachedSprites.Add(itemContent.Id, handle.Result);

                    if (_cachedSprites.Count == 2)
                    {
                        Data.Working = false;
                    }
                };
            }
            catch (Exception)
            {
                OnLog("<color=#FF0000>Create federated item content in ContentManager " +
                      $"and set a reference in {nameof(PolygonAuthExample)}</color>");
            }
        }

        public override void OnRefresh()
        {
            _getInventoryButton.interactable = !Data.Working;
        }



        private async void OnGetInventoryClicked()
        {
            Data.Working = true;

            ClearItems();

            InventoryView view = await Ctx.Api.InventoryService.GetCurrent();

            ParseCurrencies(view.currencies);
            ParseItems(view.items);

            void ParseCurrencies(Dictionary<string, long> currencies)
            {
                StringBuilder builder = new();
                foreach (var (currency, amount) in currencies)
                {
                    if (!_cachedSprites.TryGetValue(currency, out Sprite sprite)) continue;

                    Instantiate(_itemPresenter, _itemsParent, false).GetComponent<ItemPresenter>()
                        .Setup(sprite, amount.ToString());

                    builder.AppendLine($"Currency: {currency}, amount: {amount}");
                }

                if (builder.Length > 0)
                {
                    OnLog.Invoke(builder.ToString());
                }
            }

            Data.Working = false;

            void ParseItems(Dictionary<string, List<ItemView>> items)
            {
                StringBuilder builder = new();

                foreach (var (itemId, itemInstances) in items)
                {
                    if (!_cachedSprites.TryGetValue(itemId, out Sprite sprite)) continue;

                    Instantiate(_itemPresenter, _itemsParent, false).GetComponent<ItemPresenter>()
                        .Setup(sprite, itemInstances.Count.ToString());

                    builder.AppendLine($"Item: {itemId}, amount: {itemInstances.Count}");
                }

                if (builder.Length > 0)
                {
                    OnLog.Invoke(builder.ToString());
                }
            }
        }

        private void ClearItems()
        {
            foreach (Transform child in _itemsParent)
            {
                Destroy(child.gameObject);
            }
        }
    }
}