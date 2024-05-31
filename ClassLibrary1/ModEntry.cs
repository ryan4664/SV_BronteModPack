using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;

namespace ClassLibrary1
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        // The location of the coin somehow, look for a community one
        private readonly Rectangle CoinSourceRect = new(5, 69, 6, 6);
        private readonly Rectangle TooltipSourceRect = new(0, 256, 60, 60);

        private const int TooltipBorderSize = 12;
        private const int Padding = 10;
        private readonly Vector2 TooltipOffset = new(Game1.tileSize / 2);

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {

            helper.Events.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
        }


        /// <summary>When a menu is open (<see cref="Game1.activeClickableMenu"/> isn't null), raised after that menu is drawn to the sprite batch but before it's rendered to the screen.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
        {
            // get item
            Item? item = this.GetItemFromMenu(Game1.activeClickableMenu);

            if (item == null)
                return;

            this.IsItemInBundle(item);

            Monitor.LogOnce($"Item: {item.ItemId}", LogLevel.Info);

            // draw tooltip
            this.DrawPriceTooltip(Game1.spriteBatch, Game1.smallFont, item);
        }

        /// <summary>Get the hovered item from an arbitrary menu.</summary>
        /// <param name="menu">The menu whose hovered item to find.</param>
        private Item? GetItemFromMenu(IClickableMenu menu)
        {
            // game menu
            // This is if we are in the menu that shows all items, implement later
            if (menu is GameMenu gameMenu)
            {
                IClickableMenu page = this.Helper.Reflection.GetField<List<IClickableMenu>>(gameMenu, "pages").GetValue()[gameMenu.currentTab];
                if (page is InventoryPage)
                    return this.Helper.Reflection.GetField<Item>(page, "hoveredItem").GetValue();
                else if (page is CraftingPage)
                    return this.Helper.Reflection.GetField<Item>(page, "hoverItem").GetValue();
            }
            // from inventory UI
            else if (menu is MenuWithInventory inventoryMenu)
            {
                return inventoryMenu.hoveredItem;
            }

            return null;
        }

        /// <summary>Draw a tooltip box which shows the unit and stack prices for an item.</summary>
        /// <param name="spriteBatch">The sprite batch to update.</param>
        /// <param name="font">The font with which to draw text.</param>
        /// <param name="item">The item whose price to display.</param>
        private void DrawPriceTooltip(SpriteBatch spriteBatch, SpriteFont font, Item item)
        {
            // get info
            int stack = item.Stack;
            int? price = 10; //this.GetSellPrice(item);
            if (price == null)
                return;

            // basic measurements
            const int borderSize = ModEntry.TooltipBorderSize;
            const int padding = ModEntry.Padding;
            int coinSize = this.CoinSourceRect.Width * Game1.pixelZoom;
            int lineHeight = (int)font.MeasureString("X").Y;
            Vector2 offsetFromCursor = this.TooltipOffset;
            bool showStack = stack > 1;

            // prepare text
            string label = "Yes";

            // get dimensions
            Vector2 labelSize = font.MeasureString(label);
            Vector2 innerSize = new(labelSize.X + padding + coinSize, labelSize.Y);
            Vector2 outerSize = innerSize + new Vector2((borderSize + padding) * 2);

            // get position
            float x = Game1.getMouseX() - offsetFromCursor.X - outerSize.X;
            float y = Game1.getMouseY() + offsetFromCursor.Y + borderSize;

            // adjust position to fit on screen
            Rectangle area = new((int)x, (int)y, (int)outerSize.X, (int)outerSize.Y);
            if (area.Right > Game1.uiViewport.Width)
                x = Game1.uiViewport.Width - area.Width;
            if (area.Bottom > Game1.uiViewport.Height)
                y = Game1.uiViewport.Height - area.Height;

            // draw tooltip box
            IClickableMenu.drawTextureBox(spriteBatch, Game1.menuTexture, this.TooltipSourceRect, (int)x, (int)y, (int)outerSize.X, (int)outerSize.Y, Color.White);

            // draw coins
            spriteBatch.Draw(Game1.debrisSpriteSheet, new Vector2(x + outerSize.X - borderSize - padding - coinSize, y + borderSize + padding), this.CoinSourceRect, Color.White, 0.0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 1f);

            // draw text
            Utility.drawTextWithShadow(spriteBatch, label, font, new Vector2(x + borderSize + padding, y + borderSize + padding), Game1.textColor);        
        }

        private bool IsItemInBundle(Item item)
        {
            Dictionary<string, string> bundleData = Game1.netWorldState.Value.BundleData;

            Monitor.LogOnce($"ItemId {item.ItemId}", LogLevel.Info);
            Monitor.LogOnce($"Name {item.Name}", LogLevel.Info);

            int count = 0;
            foreach (var kvp in bundleData)
            {
                Monitor.LogOnce($"Key: {kvp.Key}", LogLevel.Info);
                Monitor.LogOnce($"Value: {kvp.Value}", LogLevel.Info);

                count++;
                if (count == 6)
                {
                    break;
                }
            }

            return true;
        }

    }
}