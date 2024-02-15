using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace AE.Core.Serializer
{
    /// <summary>
    /// Serializer
    /// </summary>
    public partial class AESerializer : IDisposable
	{
		private const char FIRST_ST = '~';
        private const string STRING_T = "~[";

        private class ReferenceObj
        {
            public int Id { get; set; }
            public bool HasReference { get; set; }
            public object Obj { get; set; }
		}

		private StringBuilder Builder { get; set; }
		private List<string> TypeTable { get; set; }
        private List<ReferenceObj> ReferenceTable { get; set; }

        private void Init()
        {
			Builder = new StringBuilder();
			TypeTable = new List<string>();
			ReferenceTable = new List<ReferenceObj>();
		}

		private Type GetSaveType(string id)
        {
            if (id.TryInt(out int index) && index < TypeTable?.Count)
                id = TypeTable[index];

            return id.Type();
        }

        private int SetSaveTypeId(object obj)
            => SetSaveTypeId(obj.GetType());

        private int SetSaveTypeId(Type type)
        {
            if (!TypeTable.Contains(type.FullName))
                TypeTable.Add(type.FullName);

            return TypeTable.IndexOf(type.FullName);
        }

		private object GetReferenceObject(string refId)
        {
			if (refId.TryInt(out int id))
				return ReferenceTable.FirstOrDefault(r => r.Id == id)?.Obj;

            return null;
		}

		private int GetReferenceObjectId(object obj)
		{
			var reference = ReferenceTable.FirstOrDefault(r => ReferenceEquals(r.Obj, obj));

			if (reference == null)
				return -1;

			reference.HasReference = true;
			return reference.Id;
		}

		private bool SetReferenceObject(object obj)
        {
            var reference = ReferenceTable.FirstOrDefault(r => ReferenceEquals(r.Obj, obj));

            if (reference == null)
                return SetReferenceObject(ReferenceTable.Count, obj);

			return false;
		}

		private bool SetReferenceObject(int id, object obj)
        {
			ReferenceTable.Add(new ReferenceObj
            {
                Id = id,
				HasReference = false,
                Obj = obj,
            });

            return true;
		}

		/// <inheritdoc/>
		public void Dispose()
        {
            TypeTable?.Clear();
			ReferenceTable?.Clear();
			Builder?.Clear();
		}
    }
}
