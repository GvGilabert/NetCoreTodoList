using AspNetCoreTodo.Services;

namespace AspNetCoreTodo.Models
{
    public class AddressViewModel
    {
        public info info { get; set; }
    }

    public class info
    {
        public results[] results;
    }
    public class results
    {
        public locations[] locations;
    }

    public class locations
    {
        public string adminArea5 {get;set;}
        public string street { get; set; }
        public latLng latLng { get; set; }
        public string mapUrl { get; set; }
        public string adminArea1 {get;set;}
    }

    public class latLng
    {

        public double lat { get; set; }
        public double lng { get; set; }

    }
}