using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRCode
{
    public class QRCodeItem
    {
        public int Id {  get; set; }
        public ImageSource QrCodeImg { get; set; }
        public string QrCodeBase64 { get; set; }
    }
}
