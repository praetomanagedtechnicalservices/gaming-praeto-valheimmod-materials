using System.IO;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using SuijinMaterials.Assets;
using UnityEngine;

namespace SuijinMaterials.Content
{
    internal sealed class JotunnMaterialRegistrationBackend : IMaterialRegistrationBackend
    {
        public void Register(MaterialDefinition definition, string iconPath)
        {
            string assetError = IconAsset.ValidatePng(iconPath);
            if (assetError != null)
            {
                throw new InvalidDataException(assetError);
            }

            Sprite icon = AssetUtils.LoadSpriteFromFile(iconPath);
            if (icon == null)
            {
                throw new InvalidDataException($"PNG asset '{iconPath}' could not be loaded as a Unity sprite.");
            }

            var itemConfig = new ItemConfig
            {
                Name = definition.NameToken,
                Description = definition.DescriptionToken,
                Icon = icon
            };

            var item = new CustomItem(definition.PrefabId, definition.CloneSourcePrefab);
            itemConfig.Apply(item.ItemPrefab);
            if (!ItemManager.Instance.AddItem(item))
            {
                throw new InvalidDataException($"Jötunn rejected custom item '{definition.PrefabId}'.");
            }
        }
    }
}
