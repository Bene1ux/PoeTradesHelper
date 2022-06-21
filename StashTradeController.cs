﻿using System;
using System.Collections.Generic;
using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Enums;
using SharpDX;
using StashItemsDict = System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<int>>;

namespace PoeTradesHelper
{
    using System.Diagnostics;
    using System.Linq;
    using ExileCore.PoEMemory.Elements.InventoryElements;

    public class StashTradeController
    {
        private readonly GameController _gameController;
        private readonly Graphics _graphics;

        private Dictionary<string, StashItemsDict> _itemPrices = new Dictionary<string, StashItemsDict>();

        public StashTradeController(GameController gameController, Graphics graphics)
        {
            _gameController = gameController;
            _graphics = graphics;
        }

        public void Draw(ICollection<TradeEntry> entries)
        {
            HighlightTradeItems(entries);
            //UpdateStashTradeItems();
        }

        public void HighlightCell(int x, int y, bool isQuad)
        {
            var stashElement = _gameController.Game.IngameState.IngameUi.StashElement;

            if (!stashElement.IsVisible)
                return;

            var visibleStash = stashElement.VisibleStash;
            if (visibleStash != null)
            {
                var items = stashElement.VisibleStash?.VisibleInventoryItems;
                if (items != null)
                {
                    isQuad = isQuad || items.Any(i => i.InventPosX > 12 || i.InventPosY > 12);
                }

                var cellCount = isQuad ? 24 : 12;
                var visibleStashRect = visibleStash.GetClientRectCache;
                var cellSize = visibleStashRect.Width / cellCount;

                var itemRect = new RectangleF(
                    visibleStashRect.X + (x - 1) * cellSize,
                    visibleStashRect.Y + (y - 1) * cellSize,
                    cellSize,
                    cellSize);

                _graphics.DrawFrame(itemRect, Color.Azure, 3);
            }
        }
        
        private void HighlightTradeItems(ICollection<TradeEntry> entries)
        {
            var stashElement = _gameController.Game.IngameState.IngameUi.StashElement;

            if (!stashElement.IsVisible)
                return;

            var viewAllStashPanel = stashElement.ViewAllStashPanel;
            var yShift = _gameController.Memory.Read<float>(viewAllStashPanel.Address + 0x5C);
            var stashNames = stashElement.AllStashNames;
            var currentStash = stashElement.IndexVisibleStash;
            var visibleStash = stashElement.VisibleStash;

            foreach (var tradeEntry in entries)
            {
                if (!tradeEntry.IsIncomingTrade)
                    continue;

                if (tradeEntry.ItemPosInfo == null)//try draw without pos
                {
                    var items = stashElement.VisibleStash?.VisibleInventoryItems;

                    if (items != null)
                    {
                        var tradeItems = items.Where(x => tradeEntry.ItemName.Contains(GetItemName(x.Item)));

                        foreach (var item in tradeItems)
                        {
                            _graphics.DrawFrame(item.GetClientRect(), Color.Magenta, 2);
                        }
                    }
                    
                    continue;
                }

                var index = stashNames.IndexOf(tradeEntry.ItemPosInfo.TabName);

                if (index != currentStash)
                {
                    if (viewAllStashPanel.IsVisible && index != -1)
                    {
                        var childAtIndex = stashElement.ViewAllStashPanelChildren[index];

                        if (childAtIndex != null)
                        {
                            var rect = childAtIndex.GetClientRect();
                            rect.Y += yShift * viewAllStashPanel.Scale;
                            _graphics.DrawFrame(rect, Color.Yellow, 2);
                        }
                        else
                        {
                            Logger.Log.Error($"TradeController: No child at {index}");
                        }
                    }
                }
                else if(visibleStash != null)
                {
                    var items = stashElement.VisibleStash?.VisibleInventoryItems;
                    var isQuad = false;
                    if (items != null)
                    {
                        isQuad = items.Any(i => i.InventPosX > 12 || i.InventPosY > 12);
                    }

                    var cellCount = isQuad ? 24 : visibleStash.InvType == InventoryType.QuadStash ? 24 : 12;
                    var visibleStashRect = visibleStash.GetClientRectCache;
                    var cellSize = visibleStashRect.Width / cellCount;

                    var itemRect = new RectangleF(
                        visibleStashRect.X + (tradeEntry.ItemPosInfo.Pos.X - 1) * cellSize,
                        visibleStashRect.Y + (tradeEntry.ItemPosInfo.Pos.Y - 1) * cellSize,
                        cellSize,
                        cellSize);

                    _graphics.DrawFrame(itemRect, Color.Yellow, 3);
                }
            }
        }

        private void UpdateStashTradeItems()
        {
            var stashElement = _gameController.Game.IngameState.IngameUi.StashElement;

            if (!stashElement.IsVisible)
                return;

            var items = stashElement.VisibleStash?.VisibleInventoryItems;

            if (items == null)
                return;

            var priceDict = new StashItemsDict();

            foreach (var normalInventoryItem in items)
            {
                try
                {
                    var item = normalInventoryItem.Item;
                    if (item == null)
                        continue;

                    var baseComp = item.GetComponent<Base>();
                    if (string.IsNullOrEmpty(baseComp.PublicPrice))
                        continue;

                    var itemName = GetItemName(item);

                    if (priceDict.TryGetValue(itemName, out var list))
                    {
                    }
                }
                catch (Exception e)
                {
                }
            }
        }

        private static string GetItemName(Entity entity)
        {
            var name = string.Empty;

            // var mods = entity.GetComponent<Mods>();
            // if (mods != null)
            // {
            //     name += $"{mods.UniqueName} ";
            // }

            var baseComp = entity.GetComponent<Base>();

            if (baseComp != null)
            {
                name += baseComp.Name;
            }

            return name;
        }

        //public List<int> GetItemAmount(string itemName)
        //{

        //}
    }
}