using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Collections;
using Microsoft.EnterpriseSingleSignOn.Interop;
using BizTalk.Tools.SSOStorage;

namespace Kelly.SSOImport
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //Get XML Config File....
                string strPath;
                if (args.Length == 0)
                {
                    Console.WriteLine("Please specify the XML Config file to be imported as parameter and hit enter");
                    strPath = Console.ReadLine();
                }
                else
                    strPath = args[0];

                XPathDocument xpathDoc = new XPathDocument(strPath);
                XPathNavigator nav = xpathDoc.CreateNavigator();

                //- Retrieve Values from the XML...
                string appName = nav.SelectSingleNode("/sso/application/@name").Value;
                string desc = nav.SelectSingleNode("/sso/application/description").Value;
                string appUserAccount = nav.SelectSingleNode("/sso/application/appUserAccount").Value;
                string appAdminAccount = nav.SelectSingleNode("/sso/application/appAdminAccount").Value;

                //Give Opt. to del for any existing App
                Console.Write("This App will overwrite if there is any existing SSO App:" + appName + ". Do you want to continue? (Y/N)");

                ConsoleKeyInfo cki;
                cki = Console.ReadKey();
                if (cki.Key.ToString() == "N")
                    return;

                //Del existing SSO App First

                SSOConfigManager.DeleteApplication(appName);

                //- read settings into PropertyBag
                
                SSOPropBag propertiesBag = new SSOPropBag();
                ArrayList maskArray = new ArrayList();

                XPathNodeIterator fieldNodes = nav.SelectSingleNode("/sso/application").SelectChildren("field", "");
                while (fieldNodes.MoveNext())
                {
                    string key = fieldNodes.Current.GetAttribute("label", "");
                    string value = fieldNodes.Current.InnerXml;
                    object objVal = value;
                    propertiesBag.Write(key, ref objVal);
                    maskArray.Add(0);
                }

                //create and enable application
                SSOConfigManager.CreateConfigStoreApplication(appName, desc, appUserAccount, appAdminAccount, propertiesBag, maskArray);
                SSOConfigManager.SetConfigProperties(appName, propertiesBag);
                
                Console.WriteLine("");
                Console.WriteLine("Application " + appName + " created successfully in SSO");
                Console.ReadKey();
            }

            catch (Exception ex)
            {
                Console.Write("Error Occured.  Details: " + ex.ToString());
            }

        }
    }
}
