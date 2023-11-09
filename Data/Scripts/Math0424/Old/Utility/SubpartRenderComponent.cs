using Sandbox.Game.Components;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game.Entity;
using VRageMath;

namespace AnimationEngine.Utility
{
    //copy paste of MyParentedSubpartRenderComponent
    internal class SubpartRenderComponent : MyRenderComponent
    {

		public override void OnAddedToContainer()
		{
			base.OnAddedToContainer();
			MyEntity myEntity = (MyEntity)base.Entity;
			myEntity.InvalidateOnMove = false;
			myEntity.NeedsWorldMatrix = false;
		}

		public override void AddRenderObjects()
		{
			base.AddRenderObjects();
			this.UpdateParent();
		}

		public void UpdateParent()
		{
			if (base.GetRenderObjectID() != 4294967295U)
			{
				uint num = base.Entity.Parent.Render.ParentIDs[0];
				if (num != 4294967295U)
				{
					Matrix value;
					this.GetCullObjectRelativeMatrix(out value);
					base.SetParent(0, num, new Matrix?(value));
				}
			}
		}

		public void GetCullObjectRelativeMatrix(out Matrix relativeMatrix)
		{
			relativeMatrix = base.Entity.PositionComp.LocalMatrixRef * base.Entity.Parent.PositionComp.LocalMatrixRef;
		}

	}
}
