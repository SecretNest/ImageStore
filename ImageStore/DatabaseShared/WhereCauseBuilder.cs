using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.DatabaseShared
{

    class WhereCauseBuilder
    {
        SqliteParameterCollection parameters; List<string> whereCauses; bool allMet;
        public WhereCauseBuilder(SqliteParameterCollection parameters, bool allMet = true)
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
                parameters.AddBlob("@" + columnName, value);
            }
        }

        internal void AddUniqueIdentifierComparingCause(string columnName, Guid? value)
        {
            if (value.HasValue)
            {
                whereCauses.Add(string.Format("[{0}] = @{0}", columnName));
                parameters.AddGuid("@" + columnName, value.Value);
            }
        }

        internal void AddIntInRangeCause(string columnName, List<int> values)
        {
            if (values == null || values.Count == 0)
                return;

            if (values.Count == 1)
            {
                //Falling through would append the same predicate a second time, as a
                //one-element or group with its own parameter. Harmless but wasteful.
                AddIntComparingCause(columnName, values[0]);
                return;
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
                parameters.AddInt("@" + parameterName, value.Value);
            }
        }

        internal void AddIntComparingCause(string columnName, int? value, int? greaterOrEqual, int? lessOrEqual)
        {
            if (value.HasValue)
            {
                whereCauses.Add(string.Format("[{0}] = @{0}", columnName));
                parameters.AddInt("@" + columnName, value.Value);
            }
            else
            {
                if (greaterOrEqual.HasValue)
                {
                    whereCauses.Add(string.Format("[{0}] >= @GreaterOrEqual{0}", columnName));
                    parameters.AddInt("@GreaterOrEqual" + columnName, greaterOrEqual.Value);
                }

                if (lessOrEqual.HasValue)
                {
                    whereCauses.Add(string.Format("[{0}] <= @LessOrEqual{0}", columnName));
                    parameters.AddInt("@LessOrEqual" + columnName, lessOrEqual.Value);
                }
            }
        }

        internal void AddBitComparingCause(string columnName, bool? value)
        {
            if (value.HasValue)
            {
                whereCauses.Add(string.Format("[{0}] = @{0}", columnName));
                parameters.AddBool("@" + columnName, value.Value);
            }
        }

        internal void AddRealComparingCause(string columnName, float? value, float? greaterOrEqual, float? lessOrEqual)
        {
            if (value.HasValue)
            {
                whereCauses.Add(string.Format("[{0}] = @{0}", columnName));
                parameters.AddReal("@" + columnName, value.Value);
            }
            else
            {
                if (greaterOrEqual.HasValue)
                {
                    whereCauses.Add(string.Format("[{0}] >= @GreaterOrEqual{0}", columnName));
                    parameters.AddReal("@GreaterOrEqual" + columnName, greaterOrEqual.Value);
                }

                if (lessOrEqual.HasValue)
                {
                    whereCauses.Add(string.Format("[{0}] <= @LessOrEqual{0}", columnName));
                    parameters.AddReal("@LessOrEqual" + columnName, lessOrEqual.Value);
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
                        whereCauses.Add(string.Format("[{0}] like @{1}" + SqliteLikeValueBuilder.EscapeClause, columnName, parameterName));
                        parameters.AddText("@" + parameterName, SqliteLikeValueBuilder.EscapeAndInclude(value));
                    }
                    else if (comparingModes == StringPropertyComparingModes.Equals)
                    {
                        whereCauses.Add(string.Format("[{0}] = @{1}", columnName, parameterName));
                        parameters.AddText("@" + parameterName, value);
                    }
                    else if (comparingModes == StringPropertyComparingModes.StartsWith)
                    {
                        whereCauses.Add(string.Format("[{0}] like @{1}" + SqliteLikeValueBuilder.EscapeClause, columnName, parameterName));
                        parameters.AddText("@" + parameterName, SqliteLikeValueBuilder.Escape(value) + "%");
                    }
                    else if (comparingModes == StringPropertyComparingModes.EndsWith)
                    {
                        whereCauses.Add(string.Format("[{0}] like @{1}" + SqliteLikeValueBuilder.EscapeClause, columnName, parameterName));
                        parameters.AddText("@" + parameterName, "%" + SqliteLikeValueBuilder.Escape(value));
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
