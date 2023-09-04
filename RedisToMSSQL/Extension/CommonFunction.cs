using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisToMSSQL.Extension
{
    public class CommonFunction
    {
        public static string GenerateInserScript(object sample, string[] ignoredColNames = null)
        {
            var sb = new StringBuilder();
            sb.AppendLine("INSERT INTO " + sample.GetType().Name);

            string[] props = sample.GetType()
                .GetProperties()
                .Select(o => o.Name).ToArray();
            props = props.Where(o =>
                !o.StartsWith("_") && //排除_開頭及ignoredColNames列舉欄位名
                (ignoredColNames == null || !ignoredColNames.Contains(o)))
                .ToArray();
            sb.AppendLine($"({string.Join(", ", props)})");
            sb.AppendLine("VALUES");
            sb.AppendLine($"({string.Join(", ", props.Select(o => "@" + o).ToArray())});");
            return sb.ToString();
        }
    }
}
