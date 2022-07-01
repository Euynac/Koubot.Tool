namespace Koubot.Tool.Model
{
    ///// <summary>
    ///// 过滤类型
    ///// </summary>
    //[Flags]
    //public enum FilterType
    //{
    //    /// <summary>
    //    /// 默认（String默认为模糊并忽略大小写，int以及double为区间支持型匹配，其他类型为精确匹配）
    //    /// </summary>
    //    Default = 0,
    //    /// <summary>
    //    /// 仅忽略大小写
    //    /// </summary>
    //    IgnoreCase = 1 << 0,
    //    /// <summary>
    //    /// 模糊匹配（会忽略两边空格）
    //    /// </summary>
    //    Fuzzy = 1 << 1,
    //    /// <summary>
    //    /// 模糊并忽略大小写
    //    /// </summary>
    //    FuzzyIgnoreCase = Fuzzy | IgnoreCase,
    //    /// <summary>
    //    /// 精确匹配
    //    /// </summary>
    //    Exact = 1 << 2,
    //    /// <summary>
    //    /// 针对源数据为字符串型数字，包含范围、区间型值(闭区间)（即格式为 num1分隔符num2 的string类型）
    //    /// </summary>
    //    Interval = 1 << 3,
    //    /// <summary>
    //    /// 支持空值匹配
    //    /// </summary>
    //    SupportNull = 1 << 4,
    //    /// <summary>
    //    /// string类型支持""值匹配
    //    /// </summary>
    //    SupportStrEmpty = 1 << 5,
    //    /// <summary>
    //    /// 支持默认值匹配（默认会过滤掉为默认值的，比如0）
    //    /// </summary>
    //    SupportValueDefault = 1 << 6,
    //    /// <summary>
    //    /// [非用户用] 自动过滤空值（Sorter专用，意思是仅排序，filter仅过滤无效值和默认值）
    //    /// </summary>
    //    OnlyFilterNull = 1 << 7,
    //    /// <summary>
    //    /// 让使用的Sorter不要自动添加过滤器
    //    /// </summary>
    //    SorterNoFilterNull = 1 << 8,

    //}
    ///// <summary>
    ///// 排序类型，默认不排序
    ///// </summary>
    //public enum SortType
    //{
    //    /// <summary>
    //    /// 不排序
    //    /// </summary>
    //    None,
    //    /// <summary>
    //    /// 升序排序
    //    /// </summary>
    //    Ascending,
    //    /// <summary>
    //    /// 降序排序
    //    /// </summary>
    //    Descending,
    //    /// <summary>
    //    /// 指示该字段支持字符串型排序（认为ASC、DESC/DSC、升序、降序这四个字段为关键字）
    //    /// </summary>
    //    StringAuto,
    //}
    ///// <summary>
    ///// 针对一个Model类的搜索过滤器容器，能够提高效率，并提供自动化的泛型Filter
    ///// </summary>
    ///// <typeparam name="T"></typeparam>
    //[Obsolete("效率过低，已废弃")]
    //public class FilterContainer<T> where T : class
    //{
    //    /// <summary>
    //    /// 容器filter Invoke列表 [Key:Filter, Value:传入Filter的值]
    //    /// </summary>
    //    private readonly List<KeyValuePair<Func<T, object, bool>, object>> filterInvokeList = new List<KeyValuePair<Func<T, object, bool>, object>>();


    //    #region 泛型ModelFilter、ModelSorter
    //    /// <summary>
    //    /// 已加入的泛型ModelFilter [Key属性名 Value[Key属性值,Value过滤类型]]
    //    /// </summary>
    //    public Dictionary<string, KeyValuePair<object, FilterType>> ModelFilterDict { get; private set; } = new Dictionary<string, KeyValuePair<object, FilterType>>();

    //    /// <summary>
    //    /// 自动Model排序规则列表，若Dict中有某字段则说明该字段要排序。默认升序，可以指定降序[KeyModel字段名,Value[key权重,value是否降序]] 权重越大越先排
    //    /// </summary>
    //    public Dictionary<string, KeyValuePair<int, bool>> ModelSortRuleDict { get; private set; } = new Dictionary<string, KeyValuePair<int, bool>>();
    //    /// <summary>
    //    /// 在泛型ModelFilter列表中添加一个字段的过滤规则，最后需要调用AddModelFilterIntoContainer将其添加到Filter容器里，不然不会生效
    //    /// </summary>
    //    /// <param name="modelFieldName">Model对应的字段名，使用namof(Model.字段名)</param>
    //    /// <param name="compareValue">传入与Model字段需要比较的值</param>
    //    /// <param name="fieldFilterMode">指定字段过滤模式（模糊以及忽略大小写仅对String有效），其他类型字段默认为精确匹配，string字段默认模糊并忽略大小写匹配</param>
    //    /// <param name="sortType">指定字段排序规则，默认不排序</param>
    //    public void AddAutoModelFilter(string modelFieldName, object compareValue, FilterType fieldFilterMode = FilterType.Default, SortType sortType = SortType.None)
    //    {
    //        if (compareValue == null && !fieldFilterMode.HasFlag(FilterType.SupportNull)) return;
    //        switch (sortType)
    //        {
    //            case SortType.Ascending:
    //                ModelSortRuleDict.Add(modelFieldName, new KeyValuePair<int, bool>(1, false));
    //                break;
    //            case SortType.Descending:
    //                ModelSortRuleDict.Add(modelFieldName, new KeyValuePair<int, bool>(1, true));
    //                break;
    //            case SortType.StringAuto:
    //                if (compareValue is string sortStr)
    //                {
    //                    Regex regex = new Regex(@"[,;\\]?\d*(asc|dsc|升序|降序|desc)[,;\\]?", RegexOptions.IgnoreCase);
    //                    if (regex.IsMatch(sortStr))
    //                    {
    //                        string sortRule = regex.Match(sortStr)?.Value?.ToLower();//转小写
    //                        compareValue = regex.Replace(sortStr, "", 1);//这里替换掉以防影响过滤
    //                        if (!int.TryParse(sortRule.Match("\\d{1,3}"), out int weight)) weight = 1;//若未指定，权重默认是1，权重最多3位数
    //                        bool isDescending = sortRule.IsMatch("(dsc|降序|desc)");//指定是否为降序，否则为升序
    //                        ModelSortRuleDict.Add(modelFieldName, new KeyValuePair<int, bool>(weight, isDescending));
    //                        if (sortStr.Trim().IsMatch(@"^\d*(asc|dsc|升序|降序|desc)$")) //若该字段仅排序
    //                        {
    //                            if (fieldFilterMode.HasFlag(FilterType.SorterNoFilterNull)) return;//除非不要添加过滤无效值的过滤器
    //                            else fieldFilterMode |= FilterType.OnlyFilterNull;//默认自动添加一个过滤无效值（null）的过滤器
    //                        }
    //                    }
    //                }
    //                break;
    //            default:
    //                break;
    //        }
    //        ModelFilterDict.Add(modelFieldName, new KeyValuePair<object, FilterType>(compareValue, fieldFilterMode));
    //    }
    //    /// <summary>
    //    /// 根据放入AutoModelFilterDict的字段（要指定排序功能）自动生成一个对应的ModelSorter用于Sort排序，若SortRuleDict中没有规则，则返回null
    //    /// </summary>
    //    /// <returns></returns>
    //    public Comparison<T> GetModelSorter()
    //    {
    //        if (ModelSortRuleDict.IsNullOrEmptySet()) return null;
    //        return delegate (T modelX, T modelY)
    //        {
    //            int result = 0;//1,-1,0分别是大，小，相等，默认升序
    //            Dictionary<string, KeyValuePair<Type, object>> modelxInfoDict = ReflectionTool.GetAllPropertyInfo<T>(modelX);//获取一个model字段的信息
    //            foreach (var modelProperty in modelxInfoDict)
    //            {
    //                string fieldName = modelProperty.Key;
    //                Type type = modelProperty.Value.Key;
    //                if (ModelSortRuleDict.TryGetValue(fieldName, out KeyValuePair<int, bool> rule) && typeof(IComparable).IsAssignableFrom(type)) //若该字段有排序规则且实现了Comparable接口
    //                {
    //                    var xValue = modelProperty.Value.Value;//原始字段值
    //                    var yValue = typeof(T).GetProperty(fieldName)?.GetValue(modelY);//要比较的字段值
    //                    int weight = rule.Key;//该字段排序权重
    //                    if (rule.Value) weight = -weight;//若该字段是降序则权重相反
    //                    weight *= ((IComparable)xValue).CompareTo(yValue);
    //                    result += weight;
    //                }
    //            }
    //            return result;
    //        };
    //    }
    //    /// <summary>
    //    /// 激活自动ModelFilter，将泛型ModelFilter添加到Filter容器里
    //    /// </summary>
    //    public void ActivateAutoModelFilter()
    //    {
    //        if (!ModelFilterDict.IsNullOrEmptySet())
    //        {
    //            var func = ModelFilter();
    //            Add(func, ModelFilterDict);
    //        }
    //    }

    //    /// <summary>
    //    /// 获取一个泛型ModelFilter
    //    /// </summary>
    //    private static Func<T, object, bool> ModelFilter()
    //    {
    //        //Func<T, object, bool> func = (modelInstance, comparePair) => 的lamada表达式被简化为了本地函数
    //        static bool func(T modelInstance, object comparePair)
    //        {
    //            bool result = true;
    //            Dictionary<string, KeyValuePair<object, FilterType>> list = (Dictionary<string, KeyValuePair<object, FilterType>>)comparePair;//[Key属性名 Value[Key属性值,Value过滤类型]]
    //            Dictionary<string, KeyValuePair<Type, object>> propertyInfoDict = ReflectionTool.GetAllPropertyInfo<T>(modelInstance);
    //            if (propertyInfoDict.IsNullOrEmptySet() || comparePair == null) return true;//没东西就不用过滤了
    //            foreach (var contrastPair in list)
    //            {
    //                var pair = contrastPair.Value;
    //                if (propertyInfoDict.TryGetValue(contrastPair.Key, out KeyValuePair<Type, object> originPair))
    //                {
    //                    Type propertyType = originPair.Key;
    //                    object propertyValue = originPair.Value;
    //                    object contrast = pair.Key;
    //                    FilterType filterType = pair.Value;
    //                    var origin = propertyValue;//原始字段值


    //                    //当Str类型支持Empty值时符合则匹配
    //                    if (filterType.HasFlag(FilterType.SupportStrEmpty) && contrast is string && origin is string && ((string)contrast).IsNullOrEmpty() && ((string)origin).IsNullOrEmpty()) continue;
    //                    //原始值是空的不匹配，除非支持null值匹配
    //                    if (!filterType.HasFlag(FilterType.SupportNull) && origin == null) return false;
    //                    //原始值是值类型且是默认值的不匹配，除非支持默认值
    //                    if (origin is ValueType && !filterType.HasFlag(FilterType.SupportValueDefault) && origin.Equals(Activator.CreateInstance(propertyType))) return false;
    //                    //比较值为空的匹配，因为没有做限制，除非支持null认为null也是限制
    //                    if (!filterType.HasFlag(FilterType.SupportNull) && contrast == null) continue;
    //                    //当仅过滤空值、默认值时直接匹配（因为前面已经过滤掉了）
    //                    if (filterType.HasFlag(FilterType.OnlyFilterNull)) continue;


    //                    if (origin is string) //类型是string的
    //                    {
    //                        string ori = (string)origin;
    //                        if (ori == string.Empty && !filterType.HasFlag(FilterType.SupportStrEmpty)) return false;
    //                        string con = (string)contrast;
    //                        switch (filterType)
    //                        {
    //                            case var _ when filterType.HasFlag(FilterType.Exact): //C#7.0特性 case的when约束，要true才会成功case
    //                            case FilterType.Exact:
    //                                result &= ori.Equals(con);
    //                                break;
    //                            case FilterType.IgnoreCase:
    //                                result &= ori.Equals(con, StringComparison.OrdinalIgnoreCase);
    //                                break;
    //                            case FilterType.Fuzzy:
    //                                result &= ori.Trim().Contains(con.Trim());
    //                                break;
    //                            case FilterType.FuzzyIgnoreCase:
    //                                result &= ori.ToLower().Trim().Contains(con.ToLower().Trim());
    //                                break;
    //                            case FilterType.Interval://转为闭区间类型
    //                                result &= ori.TryGetInterval(out double left, out double right, true)
    //                                && con.TryGetInterval(out double conleft, out double conright, true)
    //                                && (conleft <= left && right <= conright);
    //                                break;
    //                            default:
    //                                goto case FilterType.FuzzyIgnoreCase;
    //                        }

    //                    }
    //                    else if (origin is double || origin is int && contrast is string)//数值型支持区间表示
    //                    {
    //                        double ori = System.Convert.ToDouble(origin);
    //                        string con = (string)contrast;
    //                        if (ZhNumber.IsContainZhNumber(con)) con = ZhNumber.ToArabicNumber(con);
    //                        if (double.TryParse(con, out double num))//直接精确等于
    //                        {
    //                            result &= num == ori;
    //                        }
    //                        else if (con.TryGetInterval(out IntervalDouble left, out IntervalDouble right))//区间表示
    //                        {
    //                            if (!(ori >= left && ori <= right)) return false;
    //                        }
    //                        else return false;//无法转换则不匹配
    //                    }
    //                    else if (origin is Enum)//支持枚举类过滤
    //                    {
    //                        if (contrast is string con)
    //                        {
    //                            result &= Enum.Parse(propertyType, con).Equals(origin);
    //                        }
    //                        else
    //                        {
    //                            result &= contrast.Equals(origin);
    //                        }
    //                    }
    //                    else if (origin is TimeSpan span && contrast is string con)//支持TimeSpan区间型过滤
    //                    {
    //                        if (con.TryGetTimeSpan(out TimeSpan timeSpan))
    //                        {
    //                            if (timeSpan != span) return false;
    //                        }
    //                        else if (con.TryGetTimeSpanInterval(out TimeSpan left, out TimeSpan right))
    //                        {
    //                            if (!(span >= left && span <= right)) return false;//不在区间内
    //                        }
    //                        else//尝试古代时间、现代时间以及中文计数法
    //                        {
    //                            var interval = Regex.Split(con, "(到|[-~,])+");
    //                            string rightCon = "";//尝试是否存在第二个以构成区间
    //                            bool isRightExist = false;
    //                            if (interval.Length >= 3)
    //                            {
    //                                con = interval[0];
    //                                rightCon = interval[2];
    //                                isRightExist = true;
    //                            }
    //                            TimeSpan zhTimeSpan = new TimeSpan();
    //                            bool success = KouStringTool.TryToTimeSpan(con, out zhTimeSpan);

    //                            if (success && isRightExist)//如果左边成功了尝试是否有第二个构成区间
    //                            {
    //                                TimeSpan zhTimeSpanRight = new TimeSpan();
    //                                bool successRight = KouStringTool.TryToTimeSpan(rightCon, out zhTimeSpanRight);
    //                                if (!successRight || zhTimeSpan > zhTimeSpanRight || span < zhTimeSpan || span > zhTimeSpanRight) return false;//原时间间隔不在区间内的不匹配
    //                            }
    //                            else if (!success || zhTimeSpan != span) return false;//时间与原时间间隔的不一致的不匹配
    //                        }
    //                    }
    //                    else//其他类型直接比较是否相等
    //                    {
    //                        contrast = System.Convert.ChangeType(contrast, propertyType);
    //                        result &= contrast.Equals(origin);
    //                    }
    //                    if (result == false) return false;
    //                }
    //            }
    //            return result;
    //        }
    //        return func;
    //    }
    //    #endregion


    //    /// <summary>
    //    /// Func强类型转换器，转换为FilterContainer支持的类型
    //    /// </summary>
    //    /// <typeparam name="TIn"></typeparam>
    //    /// <param name="func"></param>
    //    /// <returns></returns>
    //    public static Func<T, object, bool> Convert<TIn>(Func<T, TIn, bool> func)
    //    {
    //        return GenericsTool.ConvertFunc<T, TIn, object, bool>(func);
    //    }



    //    /// <summary>
    //    /// 添加一个过滤器，用于筛选时使用
    //    /// </summary>
    //    /// <param name="func">T是针对的Model类的， object是用于比较的对象，bool是比较后的返回值，用于筛选时使用</param>
    //    /// <param name="condition"> object是用于比较的条件对象</param>
    //    /// <param name="isSupportNull">默认不支持空值判断，这样可以不用手动判断是否为空了</param>
    //    public void Add(Func<T, object, bool> func, object condition, bool isSupportNull = false)
    //    {
    //        if (func == null || (condition == null && !isSupportNull)) return;
    //        filterInvokeList.Add(new KeyValuePair<Func<T, object, bool>, object>(func, condition));
    //    }


    //    /// <summary>
    //    /// 用容器里的filter开始筛选（用于linq）
    //    /// </summary>
    //    /// <param name="modelInstance"></param>
    //    /// <returns></returns>
    //    public bool StartFilter(T modelInstance)
    //    {
    //        bool result = true;
    //        foreach (var filterPair in filterInvokeList)
    //        {
    //            result &= filterPair.Key.Invoke(modelInstance, filterPair.Value);
    //            if (result == false) return false;
    //        }
    //        return true;

    //    }




    //    ///// <summary>
    //    ///// Func强类型转换器，转换为FilterContainer支持的类型
    //    ///// http://www.itkeyword.com/doc/9310190907484857895/how-to-convert-expressionfunct-object-to-expressionfuncobject-object
    //    ///// </summary>
    //    ///// <typeparam name="T2"></typeparam>
    //    ///// <param name="function"></param>
    //    ///// <returns></returns>
    //    //public static Expression<Func<T, object, bool>> ConvertFunction<T2>(Expression<Func<T, T2, bool>> function)
    //    //{
    //    //    ParameterExpression p = Expression.Parameter(typeof(object));

    //    //    return Expression.Lambda<Func<T, object, bool>>
    //    //    (
    //    //        Expression.Invoke(function, Expression.Convert(p, typeof(T2))), p
    //    //    );
    //    //}
    //}
}
