using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothesStore.Library
{
    public class NotiMessage
    {
        public string TypeNoMess {  get; set; }
        public string NotiMess { get; set; }
        public NotiMessage() { }
        public NotiMessage(string typeNoMess, string notiMess)
        {
            TypeNoMess = typeNoMess;
            NotiMess = notiMess;
        }
    }
}