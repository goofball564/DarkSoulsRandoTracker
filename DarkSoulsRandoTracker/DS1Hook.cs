using PropertyHook;
using System;
using System.Linq;

namespace DSItemTracker
{
    public class DS1Hook : PHook
    {
        private PHPointer InventoryData;
        private readonly InventoryItem[] _result;

        public DS1Hook() : base(5000, 5000, p => p.MainWindowTitle == "DARK SOULS" || p.MainWindowTitle == "DARK SOULS™: REMASTERED")
        {
            InventoryData = null;
            OnHooked += DS1Hook_OnHooked;
            OnUnhooked += DS1Hook_OnUnhooked;
            _result = Enumerable.Range( 0, 2048 ).Select( i => new InventoryItem( i ) ).ToArray();

        }

        private void DS1Hook_OnHooked(object sender, PHEventArgs e)
        {
            if (Is64Bit)
            {
                InventoryData = RegisterRelativeAOB("48 8B 05 ? ? ? ? 48 85 C0 ? ? F3 0F 58 80 AC 00 00 00", 3, 7, 0x0, 0x10, 0x3B8);
                RescanAOB();
            }
            else
            {
                InventoryData = RegisterAbsoluteAOB("A1 ? ? ? ? 53 55 8B 6C 24 10 56 8B 70 08 32 DB 85 F6", 1, 0, 8, 0x2DC);
                RescanAOB();
            }
        }

        private void DS1Hook_OnUnhooked(object sender, PHEventArgs e)
        {
            UnregisterAOBPointer((PHPointerAOB)InventoryData);
            InventoryData = null;
        }

        public InventoryItem[] GetInventoryItems()
        {
            if (InventoryData == null)
            {
                return Array.Empty<InventoryItem>();
            }
            else
            {
                byte[] bytes = InventoryData.ReadBytes(0, 2048 * 0x1C);
                for (int i = 0; i < 2048; i++)
                {
                    _result[i].Read(bytes);
                }
                return _result;
            }
        }
    }

    public struct InventoryItem
    {
        private readonly int _offset;

        public int Category;

        public int ID;

        public int Quantity;

        public int Unk0C;

        public int Unk10;

        public int Durability;

        public int Unk18;

        public InventoryItem(byte[] bytes, int index)
        {
            Category = BitConverter.ToInt32(bytes, index + 0x00) >> 28;
            ID = BitConverter.ToInt32(bytes, index + 0x04);
            Quantity = BitConverter.ToInt32(bytes, index + 0x08);
            Unk0C = BitConverter.ToInt32(bytes, index + 0x0C);
            Unk10 = BitConverter.ToInt32(bytes, index + 0x10);
            Durability = BitConverter.ToInt32(bytes, index + 0x14);
            Unk18 = BitConverter.ToInt32(bytes, index + 0x18);
            _offset = 0;
        }

        public InventoryItem( int index )
        {
            _offset = index * 0x1c;
            Category = 0;
            ID = 0;
            Quantity = 0;
            Unk0C = 0;
            Unk10 = 0;
            Durability = 0;
            Unk18 = 0;
        }       

        public void Read( byte[] bytes )
        {
            Category = BitConverter.ToInt32(bytes, _offset + 0x00) >> 28;
            ID = BitConverter.ToInt32(bytes, _offset + 0x04);
            Quantity = BitConverter.ToInt32(bytes, _offset + 0x08);
        }
    }
}