using System;
using Beamable.Common.Content;
using Beamable.Common.Inventory;
using UnityEngine;

namespace PolygonExamples.Scripts
{
	/// <summary>
	/// A script that holds the data for connected wallet and account as well as data necessary to general functioning
	/// of the example
	/// </summary>
	public class Data : MonoBehaviour
	{
		public event Action OnDataChanged;

		[SerializeField] private Federation _federation;
		[SerializeField] private CurrencyRef _currencyRef;
		[SerializeField] private ItemRef _itemRef;

		#region Auto properties

		public static Data Instance { get; private set; }
		#endregion

		#region Properties

		private bool _working;

		public bool Working
		{
			get => _working;
			set
			{
				_working = value;
				OnDataChanged?.Invoke();
			}
		}
		
		#endregion

		#region Property getters

		public bool WalletConnected => !string.IsNullOrEmpty(WalletId);
		public string WalletId { get; set; }
		public Federation Federation => _federation;
		public CurrencyRef CurrencyRef => _currencyRef;
		public ItemRef ItemRef => _itemRef;
		
		#endregion

		private void Awake()
		{
			if (Instance != null && Instance != this)
			{
				Destroy(this);
			}
			else
			{
				Instance = this;
			}
		}

		private void Start()
		{
			OnDataChanged?.Invoke();
		}
	}
}