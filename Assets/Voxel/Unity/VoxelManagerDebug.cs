using System.Collections.Generic;
using UnityEngine;
using Voxel.Blocks;
using Voxel.Core;

namespace Voxel.Unity
{
	
	[System.Serializable]
	public struct IconHelper
	{
		[SerializeField]
		public string Name;

		[SerializeField] public Sprite Icon;
	}
	
	public class VoxelManagerDebug : MonoBehaviour
	{
		
		public IconHelper[] data;
//		public IconHelper data2;
		
		// Use this for initialization
		private void Awake()
		{
			VoxelManager.Blocks.Register("grass",new GrassBlock());
			VoxelManager.Blocks.Register("dirt",new DirtBlock());
			VoxelManager.Blocks.Register("stone", new StoneBlock());
			VoxelManager.Blocks.Register("sand", new SandBlock());

			VoxelManager.Blocks.Register("coal", new CoalBlock());
			VoxelManager.Blocks.Register("copper", new CopperBlock());

			VoxelManager.Items.Register("coal", new CoalItem());
			foreach(var helper in data)
				VoxelManager.Icons.Register(helper.Name, helper.Icon);
				
			
//			VoxelManager.Items.Register("coal", new CoalItem());
//			VoxelManager.Blocks.AddReference(new LiquidBlock());
		}
	}

	public class CoalItem : ItemReference
	{
		public override Sprite Icon
		{
			get { return VoxelManager.Icons.GetReference("coal"); }
		}
	}
}
