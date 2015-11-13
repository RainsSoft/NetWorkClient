

namespace NetIOCPClient.Util.Collections
{
    #region zh-CHS 委托 | en Delegate
    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="KEY"></typeparam>
    /// <typeparam name="VALUE"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public delegate bool Predicate<KEY, VALUE>( KEY key, VALUE value );
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="KEY"></typeparam>
    /// <typeparam name="VALUE"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
   // public delegate void Action<KEY, VALUE>( KEY key, VALUE value );
    #endregion
}
