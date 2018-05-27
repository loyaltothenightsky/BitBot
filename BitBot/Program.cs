using BitMEX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace BitBot
{
    class Program
    {
        //TEST NET
        private static string TestbitmexKey = "lisa_siia_testnet_võti";
        private static string TestbitmexSecret = "lisa_siia_salatestnet_võti";
        private static string TestbitmexDomain = "https://testnet.bitmex.com";

        //REAL NET
        private static string bitmexKey = "lisa_siia_võti";
        private static string bitmexSecret = "lisa_siia_salavõti";
        private static string bitmexDomain = "https://www.bitmex.com";

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to BitBot version 1. Developed by Cult Software 2018.");
            Console.WriteLine("");

            BitMEXApi bitmex;

            List<Instrument> ActiveInstruments = new List<Instrument>();
            Instrument ActiveInstrument = new Instrument();
            List<OrderBook> CurrentBook = new List<OrderBook>();

            
            System.Timers.Timer aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEventDAtaCheck);
            aTimer.Interval = 120000;
            aTimer.Enabled = true;
            

            //-----timer for Hours opened
            System.Timers.Timer bTimer = new System.Timers.Timer();
            bTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            bTimer.Interval = 10000;
            bTimer.Enabled = true;

            bool isTestNet = true;

            double BuyPriceCurrent = 0;

            bool canGetData = true;
            bool MarketChange = true;

            double SellPrice;
            bool fetchData = true;
            bool CurrentPriceCheck = true; //reset when sold
            bool hasOpenPositions = false;
            bool CurrentPriceRestart = false;

            if (isTestNet)
            {
                while (canGetData)
                {
                    while (fetchData)
                    {
                        bitmex = new BitMEXApi(TestbitmexKey, TestbitmexSecret, TestbitmexDomain);
                    //    Console.WriteLine("is TestNet");

                        //----------Test Net
                        ActiveInstruments = bitmex.GetActiveInstruments().OrderByDescending(a => a.Volume24H).ToList();
                        ActiveInstrument = ActiveInstruments[0];

                        CurrentBook = bitmex.GetOrderBook(ActiveInstrument.Symbol, 1);

                        SellPrice = CurrentBook.Where(a => a.Side == "Sell").FirstOrDefault().Price;
                    //    Console.WriteLine("SellPrice: " + SellPrice);

                        double BuyPrice = CurrentBook.Where(a => a.Side == "Buy").FirstOrDefault().Price;
                    //    Console.WriteLine("BuyPrice: " + BuyPrice);

                        if (CurrentPriceCheck)
                        {
                            //----Initial check
                            BuyPriceCurrent = BuyPrice;
                            Console.WriteLine("BuyPriceCurrent: " + BuyPriceCurrent);

                            CurrentPriceCheck = false;
                        }

                        //-------Check market conditions
                        if (MarketChange)
                        {
                            double PriceDown = (BuyPriceCurrent - 50); 
                            if (BuyPrice <= PriceDown)
                            {
                                hasOpenPositions = true;
                                BuyPriceCurrent = BuyPrice;
                                Console.WriteLine("Time To Buy @: "+BuyPrice);
                                bitmex.MarketOrder(ActiveInstrument.Symbol, "Buy", 10);

                                MarketChange = false;
                            }

                        }

                        if (hasOpenPositions)
                        {
                            double PriceUp = (BuyPriceCurrent + 50);  
                            if (BuyPrice >= PriceUp)
                            {
                                bitmex.MarketOrder(ActiveInstrument.Symbol, "Sell", 10);

                                Console.WriteLine("Time To Sell @: "+BuyPrice);
                                CurrentPriceCheck = true;
                                MarketChange = true;
                                hasOpenPositions = false;
                            }
                        }

                        //--------If market goes up and no open positions after 2 minutes
                        if (!CurrentPriceCheck && !hasOpenPositions && CurrentPriceRestart)
                        {
                            if (BuyPrice > BuyPriceCurrent)
                            {
                                BuyPriceCurrent = BuyPrice;
                                Console.WriteLine("BuyPriceCurrent: " + BuyPriceCurrent);
                                CurrentPriceRestart = false;
                            }
                        }

                        fetchData = false;
                    }
                } 
            }   

            if (!isTestNet)
            {
                bitmex = new BitMEXApi(bitmexKey, bitmexSecret, bitmexDomain);

                //----------Real Net
            }
            
            
            void OnTimedEventDAtaCheck(object source, ElapsedEventArgs e)
            {
                CurrentPriceRestart = true;
                Console.WriteLine("Check data");
            }
            

            // Specify what you want to happen when the Elapsed event is raised.
            void OnTimedEvent(object source, ElapsedEventArgs e)
            {
                fetchData = true;
            }
        }
    }
}
