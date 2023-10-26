using System;
using Beamable;
using UnityEngine;

namespace PolygonExamples.Scripts
{
	public class TabPage : MonoBehaviour
	{
		public Action<string> OnLog;
		
		[SerializeField] private string _page;

		protected BeamContext Ctx;
		protected Data Data;

		public string Page => _page;

		private void Awake()
		{
			Ctx = BeamContext.Default;
			Data = Ctx.GetExampleData();
		}
		
		public void SetVisible(bool value)
		{
			gameObject.SetActive(value);
		}
		
		public virtual void OnRefresh() {}
	}
}