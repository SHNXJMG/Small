using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Crawler.Instance;
using Crawler;
using System.Web.Script.Serialization; 
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Net;
using Winista.Text.HtmlParser;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Filters; 
using System.Data.SqlClient;


namespace Test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        } 

        private void Form1_Load(object sender, EventArgs e)
        {
            
            Test t = new Test();
            t.Exec(); 
           
        }

        private void button1_Click(object sender, EventArgs e)
        { 
        }
    }

    public class Test : BidSzJyzxZbgg
    {
        public void Exec()
        { 
            this.ExecuteCrawl(true);
        }
    }
}
