using System;
using System.Drawing;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace Interface
{
    public partial class Ask_Gpt : Form
    {
        private static readonly string apiKey = "api_key_here";
        private static readonly string apiUrl = "https://api.openai.com/v1/chat/completions";
        string p1 = "";
        string p2 = "";
        string p3 = "";
        string p4 = "";
        string p5 = "";
        string p6 = "";
        string p7 = "";
        string p8 = "";
        string p9 = "";
        string p0 = "";
       
        public Ask_Gpt(string _p0, string _p1,string _p2,string _p3,string _p4, string _p5,
            string _p6,string _p7,string _p8, string _p9)
        {
            InitializeComponent();
            this.p1 = _p1;
            this.p2 = _p2;
            this.p3 = _p3;
            this.p4 = _p4;
            this.p5 = _p5;
            this.p6 = _p6;
            this.p7 = _p7;
            this.p8 = _p8;
            this.p9 = _p9;
            this.p0 = _p0;
            if (p0 != "" || p1 != "" || p2 != "" || p3 != "" || p4 != "" || p5 != "" || p6 != "" || p7 != "" ||
                p8 != "" || p9 != "")
            {
                prompt();
            }
        }
        private void prompt()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("--------------------------------------------------------------");
            sb.AppendLine("Process Information");
            sb.AppendLine("--------------------------------------------------------------");
            sb.AppendLine($"{"Process Name",-20}: {p0}");
            sb.AppendLine($"{"Process ID",-20}: {p1}");
            sb.AppendLine($"{"Parent Process ID",-20}: {p2}");
            sb.AppendLine($"{"Command Line",-20}: {p3}");
            sb.AppendLine($"{"CPU Usage (%)",-20}: {p4}");
            sb.AppendLine($"{"Memory Usage (MB)",-20}: {p5}");
            sb.AppendLine($"{"Network Connection",-20}: {p6}");
            sb.AppendLine($"{"Executable Path",-20}: {p7}");
            sb.AppendLine($"{"SHA-256",-20}: {p8}");
            sb.AppendLine($"{"MD5",-20}: {p9}");
            sb.AppendLine("--------------------------------------------------------------");
            sb.AppendLine("Is this file malicious or not?");
            sb.AppendLine("--------------------------------------------------------------");

            // Set the text to the textBox
            textBox1.Text = sb.ToString();

            //textBox1.Text= "Process Name:"+p0+" Process ID:"+p1+" Parent Process ID:"+p2+" Command Line:"+p3
            //    +" CPU Usage(%):"+p4+" Memory Usage(MB):"+p5+"Network Connection:"+p6+ " Executeable Path:"+p7
            //    +" SHA-256:"+p8+" MD5:"+p9 +"\n\r" +" Is this file Melicious or not?";
        }
        private async void clickOnAs(object sender, EventArgs e)
        {
            string userInput = textBox1.Text.Trim();

            if (string.IsNullOrEmpty(userInput))
            {
                MessageBox.Show("Please enter a message before clicking Ask!", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string response = await GetGptResponse(userInput);

            if (!string.IsNullOrEmpty(response))
            {
                DisplayResponse(response);
            }
            else
            {
                MessageBox.Show("Failed to get a response from GPT.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<string> GetGptResponse(string userInput)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                    var requestBody = new
                    {
                        model = "gpt-3.5-turbo",
                        messages = new[]
                        {
                            new { role = "system", content = "You are an AI assistant." },
                            new { role = "user", content = userInput }
                        },
                        max_tokens = 150
                    };

                    string json = JsonConvert.SerializeObject(requestBody);
                    StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();
                    dynamic result = JsonConvert.DeserializeObject(responseBody);

                    return result.choices[0].message.content;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private void DisplayResponse(string response)
        {
            Label responseLabel = new Label
            {
                Text = response,
                AutoSize = true,
                MaximumSize = new System.Drawing.Size(panel1.Width - 20, 0),
                Font = new Font("Microsoft Sans Serif", 10),
                ForeColor = System.Drawing.Color.Black,
                Location = new System.Drawing.Point(10, 10)
            };

            panel1.Controls.Clear(); // Clear previous responses
            panel1.Controls.Add(responseLabel);
        }
    }
}