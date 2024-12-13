using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace ClothesStore.Library
{
    public class XString
    {
        public static string Str_Slug(string s)
        {
            // Define symbol mappings (accented characters to regular characters)
            String[][] symbols = {
                new String[] { "[áàảãạâấầẩẫậăắằẳẵặ]", "a" },
                new String[] { "[đ]", "d" },
                new String[] { "[éèẻẽẹêếềểễệ]", "e" },
                new String[] { "[íìỉĩị]", "i" },
                new String[] { "[óòỏõọôốồổỗộơớờởỡợ]", "o" },
                new String[] { "[úùủũụưứừửữự]", "u" },
                new String[] { "[ýỳỷỹỵ]", "y" },
                new String[] { "[\\s'\";,]", "-" } // Convert spaces, quotes, and commas to hyphen
            };

            // Convert input string to lowercase
            s = s.ToLower();

            // Loop through each symbol array and replace occurrences in the string
            foreach (var ss in symbols)
            {
                s = Regex.Replace(s, ss[0], ss[1]);
            }

            // Return the final slug
            return s;
        }

    }
}