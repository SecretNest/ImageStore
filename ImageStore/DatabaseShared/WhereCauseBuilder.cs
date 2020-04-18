using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.DatabaseShared
{

    class WhereCauseBuilder
    {
        SqlParameterCollection parameters; List<string> whereCauses; bool allMet;
        public WhereCauseBuilder(SqlParameterCollection parameters, bool allMet = true)
        {
            this.parameters = parameters;
            whereCauses = new List<string>();
            this.allMet = allMet;
        }

        public override string ToString()
        {
            if (whereCauses.Count == 0)
                return "";
            else if (allMet)
                return string.Join(" and ", whereCauses);
            else
                return string.Join(" or ", whereCauses);
        }

        public string ToFullWhereCommand()
        {
            var causes = ToString();
            if (causes == "")
                return "";
            else
                return " where " + causes;
        }

        internal void AddPlainCause(string cause)
        {
            whereCauses.Add(cause);
        }

        internal void AddBinaryComparingCause(string columnName, byte[] value, bool isNull, int length)
        {
            if (isNull)
            {
                whereCauses.Add(string.Format("[{0}] is null", columnName));
            }
            else if (value != null)
            {
                whereCauses.Add(string.Format("[{0}] = @{0}", columnName));
                parameters.Add(new SqlParameter("@" + columnName, System.Data.SqlDbType.Binary, length) { Value = value });
            }
        }

        internal void AddUniqueIdentifierComparingCause(string columnName, Guid? value)
        {
            if (value.HasValue)
            {
                whereCauses.Add(string.Format("[{0}] = @{0}", columnName));
                parameters.Add(new SqlParameter("@" + columnName, System.Data.SqlDbType.UniqueIdentifier) { Value = value.Value });
            }
        }

        internal void AddIntInRangeCause(string columnName, List<int> values)
        {
            if (values == null || values.Count == 0)
                return;

            if (values.Count == 1)
            {
                AddIntComparingCause(columnName, values[0]);
            }

            WhereCauseBuilder inner = new WhereCauseBuilder(parameters, false);
            for (int index = 0; index < values.Count; index++)
            {
                var parameterName = columnName + index.ToString();
                inner.AddIntComparingCause(columnName, values[index], parameterName);
            }
            whereCauses.Add("(" + inner.ToString() + ")");
        }

        internal void AddIntComparingCause(string columnName, int? value)
        {
            AddIntComparingCause(columnName, value, columnName);
        }

        internal void AddIntComparingCause(string columnName, int? value, string parameterName)
        {
            if (value.HasValue)
            {
                whereCauses.Add(string.Format("[{0}] = @{1}", columnName, parameterName));
                parameters.Add(new SqlParameter("@" + parameterName, System.Data.SqlDbType.Int) { Value = value.Value });
            }
        }

        internal void AddIntComparingCause(string columnName, int? value, int? greaterOrEqual, int? lessOrEqual)
        {
            if (value.HasValue)
            {
                whereCauses.Add(string.Format("[{0}] = @{0}", columnName));
                parameters.Add(new SqlParameter("@" + columnName, System.Data.SqlDbType.Int) { Value = value.Value });
            }
            else
            {
                if (greaterOrEqual.HasValue)
                {
                    whereCauses.Add(string.Format("[{0}] >= @GreaterOrEqual{0}", columnName));
                    parameters.Add(new SqlParameter("@GreaterOrEqual" + columnName, System.Data.SqlDbType.Int) { Value = greaterOrEqual.Value });
                }

                if (lessOrEqual.HasValue)
                {
                    whereCauses.Add(string.Format("[{0}] <= @LessOrEqual{0}", columnName));
                    parameters.Add(new SqlParameter("@LessOrEqual" + columnName, System.Data.SqlDbType.Int) { Value = lessOrEqual.Value });
                }
            }
        }

        internal void AddBitComparingCause(string columnName, bool? value)
        {
            if (value.HasValue)
            {
                whereCauses.Add(string.Format("[{0}] = @{0}", columnName));
                parameters.Add(new SqlParameter("@" + columnName, System.Data.SqlDbType.Bit) { Value = value.Value });
            }
        }

        internal void AddRealComparingCause(string columnName, float? value, float? greaterOrEqual, float? lessOrEqual)
        {
            if (value.HasValue)
            {
                whereCauses.Add(string.Format("[{0}] = @{0}", columnName));
                parameters.Add(new SqlParameter("@" + columnName, System.Data.SqlDbType.Real) { Value = value });
            }
            else
            {
                if (greaterOrEqual.HasValue)
                {
                    whereCauses.Add(string.Format("[{0}] >= @GreaterOrEqual{0}", columnName));
                    parameters.Add(new SqlParameter("@GreaterOrEqual" + columnName, System.Data.SqlDbType.Real) { Value = greaterOrEqual.Value });
                }

                if (lessOrEqual.HasValue)
                {
                    whereCauses.Add(string.Format("[{0}] <= @LessOrEqual{0}", columnName));
                    parameters.Add(new SqlParameter("@LessOrEqual" + columnName, System.Data.SqlDbType.Real) { Value = lessOrEqual.Value });
                }
            }
        }

        internal void AddStringComparingCause(string columnName, string value, StringPropertyComparingModes comparingModes, int length = 256)
        {
            AddStringComparingCause(columnName, value, comparingModes, columnName, length);
        }

        void AddStringComparingCause(string columnName, string value, StringPropertyComparingModes comparingModes, string parameterName, int length)
        {
            if (value != null)
            {
                if (value == "")
                {
                    whereCauses.Add(string.Format("[{0}] = ''", columnName));
                }
                else
                {
                    if (comparingModes == StringPropertyComparingModes.Contains)
                    {
                        whereCauses.Add(string.Format("[{0}] like @{1}", columnName, parameterName));
                        parameters.Add(new SqlParameter("@" + parameterName, System.Data.SqlDbType.NVarChar, length * 3 + 2)
                        { Value = SqlServerLikeValueBuilder.EscapeAndInclude(value) });
                    }
                    else if (comparingModes == StringPropertyComparingModes.Equals)
                    {
                        whereCauses.Add(string.Format("[{0}] = @{1}", columnName, parameterName));
                        parameters.Add(new SqlParameter("@" + parameterName, System.Data.SqlDbType.NVarChar, length * 3)
                        { Value = SqlServerLikeValueBuilder.Escape(value) });
                    }
                    else if (comparingModes == StringPropertyComparingModes.StartsWith)
                    {
                        whereCauses.Add(string.Format("[{0}] like @{1}", columnName, parameterName));
                        parameters.Add(new SqlParameter("@" + parameterName, System.Data.SqlDbType.NVarChar, length * 3 + 1)
                        { Value = SqlServerLikeValueBuilder.Escape(value) + "%" });
                    }
                    else if (comparingModes == StringPropertyComparingModes.EndsWith)
                    {
                        whereCauses.Add(string.Format("[{0}] like @{1}", columnName, parameterName));
                        parameters.Add(new SqlParameter("@" + parameterName, System.Data.SqlDbType.NVarChar, length * 3 + 1)
                        { Value = "%" + SqlServerLikeValueBuilder.Escape(value) });
                    }
                    else
                    {
                        WhereCauseBuilder inner = new WhereCauseBuilder(parameters, false);
                        if (comparingModes.HasFlag(StringPropertyComparingModes.Equals))
                        {
                            inner.AddStringComparingCause(columnName, value, StringPropertyComparingModes.Equals, "Equals" + columnName, length);
                        }
                        if (comparingModes.HasFlag(StringPropertyComparingModes.Contains))
                        {
                            inner.AddStringComparingCause(columnName, value, StringPropertyComparingModes.Contains, "Contains" + columnName, length);

                        }
                        if (comparingModes.HasFlag(StringPropertyComparingModes.StartsWith))
                        {
                            inner.AddStringComparingCause(columnName, value, StringPropertyComparingModes.StartsWith, "StartsWith" + columnName, length);
                        }
                        if (comparingModes.HasFlag(StringPropertyComparingModes.EndsWith))
                        {
                            inner.AddStringComparingCause(columnName, value, StringPropertyComparingModes.EndsWith, "EndsWith" + columnName, length);
                        }
                        whereCauses.Add("(" + inner.ToString() + ")");
                    }
                }
            }
        }
    }
}
