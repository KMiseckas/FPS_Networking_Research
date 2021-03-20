using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace WorkingTitle.Enums
{
    public abstract class EnumTableEditor<TEnum, TObject> : Editor
  where TEnum : struct, Enum
    {

        static string[] _Names;
        static bool[] _Found;


        protected abstract TObject EditValue(string label, TObject value);

        public override void OnInspectorGUI()
        {
            if(_Names == null)
            {
                _Names = Enum.GetNames(typeof(TEnum));
                _Found = new bool[_Names.Length];
            }

            Array.Clear(_Found, 0, _Found.Length);

            var table = (EnumTable<TEnum, TObject>)target;
            var tableDirty = false;

            if(table.Entries == null)
            {
                table.Entries = new EnumTable<TEnum, TObject>.Entry[0];
                tableDirty    = true;
            }

            EditorGUI.BeginChangeCheck();

            // go through all entries
            for(int i = 0; i < table.Entries.Length; ++i)
            {
                var index = Array.IndexOf(_Names, table.Entries[i].Key ?? "");
                if(index < 0)
                {
                    Debug.LogError($"Removed Old \"{table.Entries[i].Key}\"");
                    ArrayUtility.RemoveAt(ref table.Entries, i--);
                    tableDirty = true;
                }
                else
                {
                    if(_Found[index])
                    {
                        Debug.LogError($"Removed Duplicate \"{table.Entries[i].Key}\"");
                        ArrayUtility.RemoveAt(ref table.Entries, i--);
                        tableDirty = true;
                    }
                    else
                    {
                        _Found[index] = true;

                        table.Entries[i].Value = EditValue(table.Entries[i].Key, table.Entries[i].Value);
                    }
                }
            }

            for(int i = 0; i < _Found.Length; ++i)
            {
                if(_Found[i] == false)
                {
                    ArrayUtility.Add(ref table.Entries, new EnumTable<TEnum, TObject>.Entry
                    {
                        Key = _Names[i]
                    });

                    tableDirty = true;
                }
            }

            if(tableDirty || EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(table);
            }
        }
    }
}
