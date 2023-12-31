//-----------------------------------------------------------------------
// <copyright file="MobileSortedDictionary.cs" company="Marimer LLC">
//     Copyright (c) Marimer LLC. All rights reserved.
//     Website: https://cslanet.com
// </copyright>
// <summary>Defines a sorted dictionary that can be serialized through</summary>
//-----------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using Csla.Serialization.Mobile;
using System;
using System.Reflection;
using Csla.Reflection;

namespace Csla.Core
{
  /// <summary>
  /// Defines a sorted dictionary that can be serialized through
  /// the SerializationFormatterFactory.GetFormatter().
  /// </summary>
  /// <typeparam name="K">Key value: any primitive or IMobileObject type.</typeparam>
  /// <typeparam name="V">Value: any primitive or IMobileObject type.</typeparam>
  [Serializable()]
  public class MobileSortedDictionary<K, V> : SortedDictionary<K, V>, IMobileObject where K : notnull
  {
    private bool _keyIsMobile;
    private bool _valueIsMobile;

    /// <summary>
    /// Creates an instance of the type.
    /// </summary>
    public MobileSortedDictionary()
    {
      DetermineTypes();
    }

    /// <summary>
    /// Creates an instance of the object based
    /// on the supplied dictionary, whose elements
    /// are copied to the new dictionary.
    /// </summary>
    /// <param name="comparer">The comparer to use when comparing keys.</param>
    public MobileSortedDictionary(IComparer<K> comparer)
      : base(comparer)
    {
      DetermineTypes();
    }

    /// <summary>
    /// Creates an instance of the object based
    /// on the supplied dictionary, whose elements
    /// are copied to the new dictionary.
    /// </summary>
    /// <param name="dict">Source dictionary.</param>
    public MobileSortedDictionary(IDictionary<K, V> dict)
      : base(dict)
    {
      DetermineTypes();
    }

    /// <summary>
    /// Creates an instance of the object based
    /// on the supplied dictionary, whose elements
    /// are copied to the new dictionary.
    /// </summary>
    /// <param name="dict">Source dictionary.</param>
    /// <param name="comparer">The comparer to use when comparing keys.</param>
    public MobileSortedDictionary(IDictionary<K, V> dict, IComparer<K> comparer)
      : base(dict, comparer)
    {
      DetermineTypes();
    }

    /// <summary>
    /// Gets a value indicating whether the
    /// dictionary contains the specified key
    /// value.
    /// </summary>
    /// <param name="key">Key value</param>
    public bool Contains(K key)
    {
      return base.ContainsKey(key);
    }

    private void DetermineTypes()
    {
      _keyIsMobile = typeof(Csla.Serialization.Mobile.IMobileObject).IsAssignableFrom(typeof(K));
      _valueIsMobile = typeof(Csla.Serialization.Mobile.IMobileObject).IsAssignableFrom(typeof(V));
    }

    #region IMobileObject Members

    private const string _keyPrefix = "k";
    private const string _valuePrefix = "v";

    void IMobileObject.GetState(SerializationInfo info)
    {
      info.AddValue("count", this.Keys.Count);
    }

    void IMobileObject.GetChildren(SerializationInfo info, MobileFormatter formatter)
    {
      int count = 0;
      foreach (var key in this.Keys)
      {
        if (_keyIsMobile)
        {
          SerializationInfo si = formatter.SerializeObject(key);
          info.AddChild(_keyPrefix + count, si.ReferenceId);
        }
        else
        {
          info.AddValue(_keyPrefix + count, key);
        }

        if (_valueIsMobile)
        {
          SerializationInfo si = formatter.SerializeObject(this[key]);
          info.AddChild(_valuePrefix + count, si.ReferenceId);
        }
        else
        {
          V value = this[key];
          info.AddValue(_valuePrefix + count, value);
        }
        count++;
      }
    }

    void IMobileObject.SetState(SerializationInfo info)
    { }

    void IMobileObject.SetChildren(SerializationInfo info, MobileFormatter formatter)
    {
      int count = info.GetValue<int>("count");

      for (int index = 0; index < count; index++)
      {
        K key;
        if (_keyIsMobile)
          key = (K)formatter.GetObject(info.Children[_keyPrefix + index].ReferenceId);
        else
          key = info.GetValue<K>(_keyPrefix + index);

        V value;
        if (_valueIsMobile)
          value = (V)formatter.GetObject(info.Children[_valuePrefix + index].ReferenceId);
        else
          value = info.GetValue<V>(_valuePrefix + index);

        Add(key, value);
      }
    }

    #endregion
  }
}