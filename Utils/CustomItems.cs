using UnityEngine;

namespace Utility
{
	public class CustomItem : Item
	{

	}
	public class ItemBuilder<I> where I : CustomItem
	{
		private Sprite smallSprite;
		private Sprite largeSprite;
		private string itemName;
		public ItemBuilder(string name)
		{
			itemName = name;
		}

		public ItemBuilder<I> Sprite(Sprite sprite)
		{
			smallSprite = sprite;
			largeSprite = sprite;
			return this;
		}
		public ItemObject Make()
		{
			ItemObject item = ScriptableObject.CreateInstance<ItemObject>();
			item.itemSpriteLarge = smallSprite;
			item.itemSpriteSmall = largeSprite;
			item.itemType = global::Items.None;
			item.nameKey = itemName;
			item.descKey = itemName;
			item.name = itemName;

			GameObject itemObj = new GameObject($"Custom_Item_{itemName}");
			GameObject.DontDestroyOnLoad(itemObj);
			itemObj.SetActive(true);

			item.item = itemObj.AddComponent<I>();
			return item;
		}
	}
}
