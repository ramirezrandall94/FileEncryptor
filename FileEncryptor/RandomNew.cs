using NLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileEncryptor
{
    public class RandomNew
    {
        public static int GetInt(int min_inclusive, int max_inclusive)
        {
            if (min_inclusive > max_inclusive)
            {
                throw new Exception(nameof(min_inclusive) + " cannot be greater than " + nameof(max_inclusive) + ".");
            }
            Lua lua = new Lua();
            lua.DoString(@"
function get_random_int(min_inclusive, max_inclusive)
    return tostring(math.random(min_inclusive, max_inclusive))
end");
            LuaFunction function = lua.GetFunction("get_random_int");
            var result = function.Call(min_inclusive, max_inclusive);
            return int.Parse((string)result[0]);
        }
    }
}
