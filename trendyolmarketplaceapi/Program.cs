using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Security.Cryptography;

namespace trendyolmarketplaceapi
{
    internal class Program
    {


        static void Main(string[] args)
        {
            //ty'den gelen açık siparişleri alıp veritabanımıza veya ERP'mize yazalım
            siparislericekdbyeyaz();


            //Trendyol 15 dakikada 1 kere stok güncellemeye müsade ettiği için Minute = 0,15,30,45 durumlarında çalıştırıyoruz
            if (DateTime.Now.Minute % 15 == 0)
            {
                TrendyolStokGuncelleme();
            }
            Console.ReadLine();
        }

        //json sınıfları
        #region json
        public class status
        {
            public string paramss { get; set; }
            public string statusOrder { get; set; }
        }
        public class Content
        {
            public ShipmentAddress shipmentAddress { get; set; }
            public string orderNumber { get; set; }
            public double grossAmount { get; set; }
            public double totalDiscount { get; set; }
            public double totalTyDiscount { get; set; }
            public object taxNumber { get; set; }
            public InvoiceAddress invoiceAddress { get; set; }
            public string customerFirstName { get; set; }
            public string customerEmail { get; set; }
            public int customerId { get; set; }
            public string customerLastName { get; set; }
            public int id { get; set; }
            public object cargoTrackingNumber { get; set; }
            public string cargoProviderName { get; set; }
            public List<Line> lines { get; set; }
            public object orderDate { get; set; }
            public string tcIdentityNumber { get; set; }
            public string currencyCode { get; set; }
            public List<PackageHistory> packageHistories { get; set; }
            public string shipmentPackageStatus { get; set; }
            public string status { get; set; }
            public string deliveryType { get; set; }
            public int timeSlotId { get; set; }
            public string scheduledDeliveryStoreId { get; set; }
            public object estimatedDeliveryStartDate { get; set; }
            public object estimatedDeliveryEndDate { get; set; }
            public double totalPrice { get; set; }
            public string deliveryAddressType { get; set; }
            public object agreedDeliveryDate { get; set; }
            public bool fastDelivery { get; set; }
            public object originShipmentDate { get; set; }
            public object lastModifiedDate { get; set; }
            public bool commercial { get; set; }
            public string fastDeliveryType { get; set; }
            public bool deliveredByService { get; set; }
        }

        public class DiscountDetail
        {
            public double lineItemPrice { get; set; }
            public double lineItemDiscount { get; set; }
            public double lineItemTyDiscount { get; set; }
        }

        public class InvoiceAddress
        {
            public object id { get; set; }
            public string firstName { get; set; }
            public string lastName { get; set; }
            public string company { get; set; }
            public string address1 { get; set; }
            public string address2 { get; set; }
            public string city { get; set; }
            public int cityCode { get; set; }
            public string district { get; set; }
            public int districtId { get; set; }
            public string postalCode { get; set; }
            public string countryCode { get; set; }
            public int neighborhoodId { get; set; }
            public string neighborhood { get; set; }
            public object phone { get; set; }
            public string fullAddress { get; set; }
            public string fullName { get; set; }
            public string taxOffice { get; set; }
            public string taxNumber { get; set; }
        }

        public class Line
        {
            public int quantity { get; set; }
            public int salesCampaignId { get; set; }
            public string productSize { get; set; }
            public string merchantSku { get; set; }
            public string productName { get; set; }
            public int productCode { get; set; }
            public int merchantId { get; set; }
            public double amount { get; set; }
            public double discount { get; set; }
            public double tyDiscount { get; set; }
            public List<DiscountDetail> discountDetails { get; set; }
            public string currencyCode { get; set; }
            public double id { get; set; }
            public string sku { get; set; }
            public double vatBaseAmount { get; set; }
            public string barcode { get; set; }
            public string orderLineItemStatusName { get; set; }
            public double price { get; set; }
        }

        public class PackageHistory
        {
            public object createdDate { get; set; }
            public string status { get; set; }
        }

        public class Root
        {
            public int totalElements { get; set; }
            public int totalPages { get; set; }
            public int page { get; set; }
            public int size { get; set; }
            public List<Content> content { get; set; }
        }

        public class Root2
        {
            public int totalElements { get; set; }
            public int totalPages { get; set; }
            public int page { get; set; }
            public int size { get; set; }
            public List<Content2> content { get; set; }
        }

        public class Content2
        {
            public string barcode { get; set; }
            public int quantity { get; set; }
            public string salePrice { get; set; }
            public string listPrice { get; set; }
            public string stockCode { get; set; }
        }

        public class ShipmentAddress
        {
            public object id { get; set; }
            public string firstName { get; set; }
            public string lastName { get; set; }
            public string company { get; set; }
            public string address1 { get; set; }
            public string address2 { get; set; }
            public string city { get; set; }
            public int cityCode { get; set; }
            public string district { get; set; }
            public int districtId { get; set; }
            public string postalCode { get; set; }
            public string countryCode { get; set; }
            public int neighborhoodId { get; set; }
            public string neighborhood { get; set; }
            public object phone { get; set; }
            public string fullAddress { get; set; }
            public string fullName { get; set; }
        }

        #endregion

        public static void siparislericekdbyeyaz()
        {
            try
            {
                if(trendyol_id == ""|| trendyol_api_key == "" || trendyol_api_secret =="")
                {
                    Console.WriteLine("Login bilgilerini giriniz");
                    return;
                }

                Console.WriteLine("Siparişleri trendyoldan api ile çekip databaseye yazma işlemi başladı.");
                var api = new Uri("https://api.trendyol.com/sapigw/suppliers/" + trendyol_id + "/orders?status=Created&size=500");

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;


                HttpWebRequest istek = (HttpWebRequest)WebRequest.Create(api);
                istek.UserAgent = trendyol_id + " - SelfIntegration";

                String encoded = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(trendyol_api_key + ":" + trendyol_api_secret));
                istek.Headers.Add("Authorization", "Basic " + encoded);

                var httpResponse = (HttpWebResponse)istek.GetResponse();

                string donus;

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    donus = streamReader.ReadToEnd();
                }
                

                Root deserializeclass = JsonConvert.DeserializeObject<Root>(donus);
                

                int linecount = deserializeclass.totalElements;

                int sayac = 0;

                foreach (var a in deserializeclass.content)
                {
                    sayac = sayac + 1;

                    long orderId = a.id;
                    string shipmentfirstname = a.shipmentAddress.firstName;
                    string shipmentlastname = a.shipmentAddress.lastName;
                    string shipmentphone = "";
                    if (a.shipmentAddress.phone != null)
                    {
                        shipmentphone = a.shipmentAddress.phone.ToString();
                    }

                    string shipmentaddress1 = a.shipmentAddress.address1;
                    string shipmentaddress2 = a.shipmentAddress.address2;
                    string shipmentcity = a.shipmentAddress.city;
                    int shipmentcitycode = a.shipmentAddress.cityCode;
                    string shipmenttown = a.shipmentAddress.district;
                    int shipmenttowncode = a.shipmentAddress.districtId;

                    string cargotrackingnr = "";
                    if (a.cargoTrackingNumber != null)
                    {
                        cargotrackingnr = a.cargoTrackingNumber.ToString();
                    }

                    string cargoProviderName = "";
                    if (a.cargoProviderName != null)
                    {
                        cargoProviderName = a.cargoProviderName.ToString();
                    }

                    string invoicefirstname = a.invoiceAddress.firstName;
                    string invoicelastname = a.invoiceAddress.lastName;
                    string invoicephone = "";
                    if (a.invoiceAddress.phone != null)
                    {
                        invoicephone = a.invoiceAddress.phone.ToString();
                    }
                    string invoiceaddress1 = a.invoiceAddress.address1;
                    string invoiceaddress2 = a.invoiceAddress.address2;
                    string invoicecity = a.invoiceAddress.city;
                    int invoicecitycode = a.invoiceAddress.cityCode;
                    string invoicetown = a.invoiceAddress.district;
                    int invoicetowncode = a.invoiceAddress.districtId;

                    double orderlineId = 0;
                    string urunbarcode = "";
                    int urunadet = 0;
                    double urunlistefiyat = 0;
                    double urunnormalindirim = 0;
                    double uruntyindirim = 0;
                    int urunsalesCampaignId = 0;
                    double urunvatBaseAmount = 0;
                    int commercial = 0;

                    if (a.commercial == true)
                    {
                        commercial = 1;
                    }

                    string taxoffice = "";
                    string taxnumber = "";

                    if (commercial == 1)
                    {
                        taxoffice = a.invoiceAddress.taxOffice;
                        taxnumber = a.invoiceAddress.taxNumber;
                    }

                    foreach (var x in a.lines)
                    {
                        orderlineId = x.id;
                        urunbarcode = x.merchantSku;
                        urunadet = x.quantity;
                        urunlistefiyat = x.price;
                        urunnormalindirim = x.discount;
                        uruntyindirim = x.tyDiscount;
                        urunsalesCampaignId = x.salesCampaignId;
                        urunvatBaseAmount = x.vatBaseAmount;


                        object[] o =

                        {
                        orderId,
                        shipmentfirstname, shipmentlastname, shipmentphone, shipmentaddress1, shipmentaddress2, shipmentcity, shipmentcitycode, shipmenttown, shipmenttowncode,
                        cargotrackingnr, cargoProviderName,
                        invoicefirstname, invoicelastname, invoicephone, invoiceaddress1, invoiceaddress2, invoicecity, invoicecitycode, invoicetown, invoicetowncode,
                        orderlineId,
                        urunbarcode, urunadet, urunlistefiyat, urunnormalindirim, uruntyindirim, urunsalesCampaignId, urunvatBaseAmount,
                        commercial, taxoffice, taxnumber
                        };

                        try
                        {
                            //o objesini veritabanınıza veya ERP'nize gönderin
                            //başarılı ise trendyolda sipariş statüsünü picking olarak güncelleyin
                            trendyolDurumGuncelle(orderId.ToString());
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine("hata: " + ex.Message);
                        }
                    }


                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("siparislericekdbyeyaz fonksiyonunda hata : " + ex.Message);
            }
        }

        public static void trendyolDurumGuncelle(string orderid)
        {



            var api = new Uri("https://api.trendyol.com/sapigw/suppliers/" + trendyol_id + "/shipment-packages/" + orderid);

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;


            HttpWebRequest istek = (HttpWebRequest)WebRequest.Create(api);
            istek.Method = "PUT";
            istek.ContentType = "application/json";
            istek.UserAgent = trendyol_id + " - SelfIntegration";

            String encoded = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(trendyol_api_key + ":" + trendyol_api_secret));
            istek.Headers.Add("Authorization", "Basic " + encoded);

            DataTable detay = new DataTable();
            //detay = veritabanınızdan orderid ile ilgili siparişin içinde hangi ürünler var, hangilerini tedarik edip statüsünü pickinge çevireceksiniz, getirin

            for (int i = 0; i < detay.Rows.Count; i++)
            {

                string requestBody = $"{{\r\n\"lines\": [\r\n{{\r\n\"lineId\": " + (long)Convert.ToDouble(detay.Rows[i]["orderlineId"]) + ", \r\n\"quantity\": " + Convert.ToInt32(detay.Rows[i]["urunadet"]) + " \r\n }\r\n],\r\n\"params\": {},\r\n\"status\": \"Picking\"\r\n}";

                byte[] byteArray = Encoding.UTF8.GetBytes(requestBody);
                istek.ContentLength = byteArray.Length;

                using (Stream dataStream = istek.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                }


            }

            var httpResponse = (HttpWebResponse)istek.GetResponse();

            string stat = httpResponse.StatusCode.ToString();
            Console.WriteLine("Trendyol Picking Güncelleme Status Kodu : " + stat);

            httpResponse.Close();
        }

        public static void TrendyolStokGuncelleme()
        {
            try
            {
                var api = new Uri("https://api.trendyol.com/sapigw/suppliers/" + trendyol_id + "/products?approved=true&size=10000");

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;


                HttpWebRequest istek = (HttpWebRequest)WebRequest.Create(api);
                istek.UserAgent = trendyol_id + " - SelfIntegration";

                String encoded = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(trendyol_api_key + ":" + trendyol_api_secret));
                istek.Headers.Add("Authorization", "Basic " + encoded);

                var httpResponse = (HttpWebResponse)istek.GetResponse();

                string donus;

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    donus = streamReader.ReadToEnd();
                }

                Root2 deserializeclass = JsonConvert.DeserializeObject<Root2>(donus);
                foreach (var a in deserializeclass.content)
                {
                    
                    int stok = 150;//veritabanınızdan veya ERP'nizden o ürünün stoğu kaç ise çağırın
                    // if(a.quantity != stok) // Burdaeğer stoklar aynı değil ise güncelleme postuna giriş yapacak.

                    //ERP'nizden fiyat güncellemek istiyorsanız saleprice ile listprice'ı burada çağırıp gönderebilirsiniz
                    TrendyolStokGüncellePostt(a.stockCode, stok, a.salePrice.Replace(",", "."), a.listPrice.Replace(",", "."));
                   
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ürün stok güncelleme de hata" + ex.Message);
            }
        }

        public static void TrendyolStokGüncellePostt(string barcode, int quantity, string salePrice, string listPrice)
        {
            try
            {
                barcode = '"' + barcode + '"';
                salePrice = '"' + salePrice + '"';
                listPrice = '"' + listPrice + '"';

                var api = new Uri("https://api.trendyol.com/sapigw/suppliers/" + trendyol_id + "/products/price-and-inventory");

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;


                HttpWebRequest istek = (HttpWebRequest)WebRequest.Create(api);
                istek.Method = "POST";
                istek.ContentType = "application/json";
                istek.UserAgent = trendyol_id + " - SelfIntegration";

                String encoded = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(trendyol_api_key + ":" + trendyol_api_secret));
                istek.Headers.Add("Authorization", "Basic " + encoded);

                string requestBody = $"{{\r\n\"items\": [\r\n{{\r\n\"barcode\": " + barcode + ",\r\n\"quantity\":" + quantity + ",\r\n\"salePrice\": " + salePrice + ",\r\n\"listPrice\":" + listPrice + "\r\n}\r\n  ]\r\n}";

                byte[] byteArray = Encoding.UTF8.GetBytes(requestBody);
                istek.ContentLength = byteArray.Length;

                using (Stream dataStream = istek.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                }
                var httpResponse = (HttpWebResponse)istek.GetResponse();

                string donus;

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    donus = streamReader.ReadToEnd();
                }

                httpResponse.Close();

                string stat = httpResponse.StatusCode.ToString();

                int start = 19;
                int end = donus.Length - start - 2;
                string data = donus.Substring(start, end);

                Console.WriteLine("Trendyol Stok Kontrol StatusCode: " + stat); // Dönüş batchId yi başka tabloyqa yazdır. TrendyolBAtcRequestId tabloya yazılacak.

                //stok ve fiyat güncelleme işleminden sonra TY bize bir batchrequest Id döndürüyor, bunları log tutturabilir veya statuscode kontrolü yaptırabilirsiniz
                //batchRequsetLog(data, "StokGüncellemePost");
            }
            catch (Exception ex)
            {
                Console.WriteLine("TrendyolStok Güncelleme POST İşleminde Hata: " + ex.Message);
            }
        }


        //TY Partner bilgileriniz
        public static string trendyol_id = "";
        public static string trendyol_api_key = "";
        public static string trendyol_api_secret = "";
    }
}
