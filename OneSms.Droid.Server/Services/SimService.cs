using Android.Content;
using Android.Net;
using OneSms.Droid.Server.Models;
using System.Collections.Generic;
using System.Diagnostics;

namespace OneSms.Droid.Server.Services
{
    public class SimService
    {
        private readonly Context _context;

        public SimService(Context context)
        {
            _context = context;
        }
        public List<SimInfo> GetSimInfos()
        {
            List<SimInfo> simInfoList = new List<SimInfo>();
            var uriTelephony = Uri.Parse("content://telephony/siminfo/");
            var c = _context.ContentResolver.Query(uriTelephony, null, null, null, null);
            if (c.MoveToFirst())
            {
                do
                {
                    int id = c.GetInt(c.GetColumnIndex("_id"));
                    int slot = c.GetInt(c.GetColumnIndex("slot"));
                    string display_name = c.GetString(c.GetColumnIndex("display_name"));
                    string icc_id = c.GetString(c.GetColumnIndex("icc_id"));
                    SimInfo simInfo = new SimInfo(id, display_name, icc_id, slot);
                    Debug.WriteLine(simInfo.ToString());
                    simInfoList.Add(simInfo);
                } while (c.MoveToNext());
            }
            c.Close();

            return simInfoList;
        }
    }
}