using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Android.Views;
using System;
using System.Text;
using Android.Support.Design.Widget;
using Xamarin.Essentials;
using Android.Text.Format;

namespace Calculator
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private TextView textViewResult;
        private TextView textViewOperationLog;
        //To keep track of numbers on which to be performed calculation
        private string[] numbers = new string[2];
        private string _operator;
        private bool isOperatorClicked = false;
        private double? result = null;
        StringBuilder operationLogBuilder = new StringBuilder();
        private Snackbar snackbar = null;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            textViewResult = FindViewById<TextView>(Resource.Id.textViewResult);
            textViewOperationLog = FindViewById<TextView>(Resource.Id.textViewOperationLog);

            textViewOperationLog.Text = "0";
            textViewResult.Text = "0";
        }

        [Java.Interop.Export("ButtonClick")]
        public void ButtonClick(View v)
        {
            try
            {
                Vibration.Vibrate(TimeSpan.FromMilliseconds(50));
            }
            catch (FeatureNotSupportedException)
            {
                snackbar = Snackbar.Make(v, "Feature Not Supported", Snackbar.LengthIndefinite).SetAction("Ok", (v) =>
                {

                });
                snackbar.Show();
                // Feature not supported on device
            }
            catch (Exception)
            {
                snackbar = Snackbar.Make(v, "Unknown Error", Snackbar.LengthIndefinite).SetAction("Ok", (v) =>
                {

                });
                snackbar.Show();
                // Other error has occurred.
            }
            /*
             * Here we're using this View v as the representative of Button in axml.
             * So we need to explicitely make this View a Button
             * Thus we have to cast this View v as Button
             */
            Button b = (Button)v;
            if ("0123456789.".Contains(b.Text))
            {
                isOperatorClicked = false;
                AddDigitOrDecimalPointByReadingFromButton(b.Text);
            }
            else if ("+-×÷".Contains(b.Text))
            {
                isOperatorClicked = true;
                AddOperatorByReadingFromButton(b.Text);
            }
            else if (b.Text.Contains("="))
            {
                isOperatorClicked = true;
                Calculate();
            }
            else if (b.Text.Contains("←"))
            {
                Erase();
            }
            else if ("C".Contains(b.Text))
            {
                ClearAll();
            }
            else if (("CE").Contains(b.Text))
            {
                ClearEntry(v);
            }
            else if ("±".Contains(b.Text))
            {
                PositiveToNegativeViceVersa();
            }
            else if ("%".Contains(b.Text))
            {
                MakeParcentage();
            }
            else if ("√".Contains(b.Text))
            {
                RootOver();
            }
            else if ("x²".Contains(b.Text))
            {
                SquareOver();
            }
            else if ("1/x".Contains(b.Text))
            {
                DividedByOne();
            }
        }

        private void AddDigitOrDecimalPointByReadingFromButton(string value)
        {
            /*
             * We will insert numbers into numbers string array
             * So we need to know first that on which position I will insert first number.
             * If there is no operator then obviously I'll insert number into 0th index
             * Otherwise in 1st index
             */
            int index = _operator == null ? 0 : 1;

            //Let's Handle the Decimal Point (.)
            if (numbers[index] != null)
            {
                if (value == "." && numbers[index].Contains("."))
                {
                    return;
                }
            }

            numbers[index] = numbers[index] + value;
            UpdateCalculatorTextView();
            operationLogBuilder.Append(value);
            textViewOperationLog.Text = operationLogBuilder.ToString();
        }

        private void UpdateCalculatorTextView() => textViewResult.Text = $"{numbers[0]} {_operator} {numbers[1]}";
        private void UpdateCalculatorOperationLogView()
        {
            operationLogBuilder.Append($"{numbers[0]} {_operator} {numbers[1]} ");
            textViewOperationLog.Text = operationLogBuilder.ToString();
        }

        private void AddOperatorByReadingFromButton(string value)
        {
            /*
             * Let's check weather there is any number into second index or not
             * If there is any number then we have to do operation on those numbers (If 1st position has any number then 0th position is always having a number)
             * Which operation to do on those numbers we will came to learn about this by reading the 
             * Then we will assign the value into _operator variable from the button text
             */
            if (numbers[1] != null)
            {
                //operationLogBuilder.Append(value);
                //textViewOperationLog.Text = operationLogBuilder.ToString();
                Calculate(value);
                operationLogBuilder.Append(value);
                textViewOperationLog.Text = operationLogBuilder.ToString();
                return;
            }

            if (numbers[0] != null)
            {
                _operator = value;
                UpdateCalculatorTextView();
                operationLogBuilder.Append(value);
                textViewOperationLog.Text = operationLogBuilder.ToString();
            }
            else
            {
                return;
            }
        }

        private void Calculate(string newOperator = null)
        {
            double? first = numbers[0] == null ? null : (double?)double.Parse(numbers[0]);
            double? last = numbers[1] == null ? null : (double?)double.Parse(numbers[1]);

            switch (_operator)
            {
                case "+":
                    result = first + last;
                    break;
                case "-":
                    result = first - last;
                    break;
                case "×":
                    result = first * last;
                    break;
                case "÷":
                    if (last != 0)
                    {
                        result = first / last;
                    }
                    else
                    {
                        result = null;
                    }
                    break;
            }

            if (result != null)
            {
                operationLogBuilder.Append("=");
                operationLogBuilder.Append(result);
                textViewOperationLog.Text = operationLogBuilder.ToString();
                numbers[0] = result.ToString();
                _operator = newOperator;
                numbers[1] = null;
                UpdateCalculatorTextView();
                //textViewOperationLog.Text = textViewOperationLog.Text + " " + $"{numbers[0]} {_operator} {numbers[1]}";


            }
            else if (result == null)
            {
                operationLogBuilder.Append("=");
                operationLogBuilder.Append("null");
                textViewOperationLog.Text = operationLogBuilder.ToString();
                numbers[0] = null;
                _operator = null;
                numbers[1] = null;
                textViewResult.Text = "Cannot divide by zero";
            }
        }

        private void Erase()
        {
            if (isOperatorClicked == true || (numbers[1] == null && numbers[0] == null))
            {
                return;
            }
            else if (isOperatorClicked == false)
            {
                if (numbers[1] != null && numbers[0] != null)
                {
                    int lengthToremove = numbers[1].Length;
                    int totalLength = operationLogBuilder.Length;
                    int startLength = totalLength - lengthToremove;
                    numbers[1] = null;
                    UpdateCalculatorTextView();
                    for (int i = startLength; i < totalLength; i++)
                    {
                        operationLogBuilder[i] = '\0';
                    }

                    textViewOperationLog.Text = operationLogBuilder.ToString();

                }
                else if (numbers[1] == null && numbers[0] != null && _operator == null)
                {
                    int lengthToremove = numbers[0].Length;
                    int totalLength = operationLogBuilder.Length;
                    int startLength = totalLength - lengthToremove;
                    numbers[0] = null;
                    UpdateCalculatorTextView();
                    for (int i = startLength; i < totalLength; i++)
                    {
                        operationLogBuilder[i] = '\0';
                    }

                    textViewOperationLog.Text = operationLogBuilder.ToString();

                }
            }
        }

        private void ClearAll()
        {
            isOperatorClicked = false;
            numbers[0] = null;
            numbers[1] = null;
            result = null;
            _operator = null;
            UpdateCalculatorTextView();
            operationLogBuilder = new StringBuilder();
            textViewOperationLog.Text = operationLogBuilder.ToString();
            textViewOperationLog.Text = "0";
            textViewResult.Text = "0";

        }

        private void ClearEntry(View view)
        {

            try
            {
                int toEliminateLength1 = numbers[0] != null ? numbers[0].Length - result.ToString().Length : 0;
                int toEliminateLength2 = 1;
                int toEliminateLength3 = numbers[1] != null ? numbers[1].Length : 0;

                int totalEliminationLength = toEliminateLength1 + toEliminateLength2 + toEliminateLength3;


                //Empty the last entry of operationBuilderLog
                for (int i = (operationLogBuilder.Length - totalEliminationLength); i < operationLogBuilder.Length; i++)
                {
                    operationLogBuilder[i] = '\0';
                }

                textViewOperationLog.Text = operationLogBuilder.ToString();

                if (!operationLogBuilder.ToString().Contains("0123456789."))
                {
                    textViewOperationLog.Text = "0";
                }

                isOperatorClicked = false;
                numbers[0] = null;
                numbers[1] = null;
                _operator = null;
                UpdateCalculatorTextView();
                textViewResult.Text = "0";
                if (result != null)
                {
                    textViewResult.Text = $"{result}";
                    numbers[0] = result.ToString();
                }
            }
            catch (Exception)
            {
                snackbar = Snackbar.Make(view, "No Entry to Clear", Snackbar.LengthIndefinite).SetAction("Ok", (v) =>
                {

                });
                snackbar.Show();
            }
        }

        private void PositiveToNegativeViceVersa()
        {
            if (numbers[0] != null && numbers[1] == null && _operator == null)
            {
                if (numbers[0].Contains("-"))
                {
                    numbers[0] = numbers[0].Replace("-", null);
                    UpdateCalculatorTextView();


                    int lengthToremove = numbers[0].Length + 1;
                    int totalLength = operationLogBuilder.Length;
                    int startLength = totalLength - lengthToremove;
                    for (int i = startLength; i < totalLength; i++)
                    {
                        operationLogBuilder[i] = '\0';
                    }
                    operationLogBuilder.Append(numbers[0]);
                    textViewOperationLog.Text = operationLogBuilder.ToString();

                }
                else if (!numbers[0].Contains("-"))
                {
                    numbers[0] = "-" + numbers[0];
                    UpdateCalculatorTextView();


                    int lengthToremove = numbers[0].Length - 1;
                    int totalLength = operationLogBuilder.Length;
                    int startLength = totalLength - lengthToremove;
                    for (int i = startLength; i < totalLength; i++)
                    {
                        operationLogBuilder[i] = '\0';
                    }
                    operationLogBuilder.Append(numbers[0]);
                    textViewOperationLog.Text = operationLogBuilder.ToString();

                }
            }
            else if (numbers[1] == null && _operator != null && numbers[0] != null)
            {
                if (numbers[0].Contains("-"))
                {
                    numbers[1] = numbers[0].Replace("-", null);
                    UpdateCalculatorTextView();



                    int lengthToremove = numbers[0].Length + 1;
                    int totalLength = operationLogBuilder.Length;
                    int startLength = totalLength - lengthToremove;
                    for (int i = startLength; i < totalLength; i++)
                    {
                        operationLogBuilder[i] = '\0';
                    }
                    operationLogBuilder.Append(numbers[0]);
                    textViewOperationLog.Text = operationLogBuilder.ToString();


                }
                else if (!numbers[0].Contains("-"))
                {
                    numbers[1] = "-" + numbers[0];
                    UpdateCalculatorTextView();


                    int lengthToremove = numbers[0].Length - 1;
                    int totalLength = operationLogBuilder.Length;
                    int startLength = totalLength - lengthToremove;
                    for (int i = startLength; i < totalLength; i++)
                    {
                        operationLogBuilder[i] = '\0';
                    }
                    operationLogBuilder.Append(numbers[0]);
                    textViewOperationLog.Text = operationLogBuilder.ToString();

                }
            }
            else if (numbers[1] != null && _operator != null && numbers[0] != null)
            {
                if (numbers[1].Contains("-"))
                {
                    numbers[1] = numbers[1].Replace("-", null);
                    UpdateCalculatorTextView();


                    int lengthToremove = numbers[1].Length + 1;
                    int totalLength = operationLogBuilder.Length;
                    int startLength = totalLength - lengthToremove;
                    for (int i = startLength; i < totalLength; i++)
                    {
                        operationLogBuilder[i] = '\0';
                    }
                    operationLogBuilder.Append(numbers[1]);
                    textViewOperationLog.Text = operationLogBuilder.ToString();

                }
                else if (!numbers[1].Contains("-"))
                {
                    numbers[1] = "-" + numbers[1];
                    UpdateCalculatorTextView();


                    int lengthToremove = numbers[1].Length - 1;
                    int totalLength = operationLogBuilder.Length;
                    int startLength = totalLength - lengthToremove;
                    for (int i = startLength; i < totalLength; i++)
                    {
                        operationLogBuilder[i] = '\0';
                    }
                    operationLogBuilder.Append(numbers[1]);
                    textViewOperationLog.Text = operationLogBuilder.ToString();

                }
            }
        }

        private void MakeParcentage()
        {
            if (numbers[0] != null && _operator != null)
            {
                if ("+-".Contains(_operator))
                {
                    //If 5+3%
                    if (numbers[1] != null)
                    {
                        double? first = numbers[0] == null ? null : (double?)double.Parse(numbers[0]);
                        double? last = numbers[1] == null ? null : (double?)double.Parse(numbers[1]);
                        result = (first * last) / 100;

                        operationLogBuilder.Append("%");
                        operationLogBuilder.Append("=");
                        operationLogBuilder.Append(result);
                        textViewOperationLog.Text = operationLogBuilder.ToString();

                        numbers[0] = result.ToString();
                        _operator = null;
                        numbers[1] = null;
                        UpdateCalculatorTextView();

                    }
                    //If 5+%
                    else if (numbers[1] == null)
                    {
                        double? first = numbers[0] == null ? null : (double?)double.Parse(numbers[0]);
                        double? last = numbers[0] == null ? null : (double?)double.Parse(numbers[0]);
                        result = (first * last) / 100;

                        operationLogBuilder.Append(last);
                        operationLogBuilder.Append("%");
                        operationLogBuilder.Append("=");
                        operationLogBuilder.Append(result);
                        textViewOperationLog.Text = operationLogBuilder.ToString();

                        numbers[0] = result.ToString();
                        _operator = null;
                        numbers[1] = null;
                        UpdateCalculatorTextView();

                    }
                }
                else if ("×÷".Contains(_operator))
                {
                    //If 5*3%
                    if (numbers[1] != null)
                    {
                        double? last = numbers[1] == null ? null : (double?)double.Parse(numbers[1]);
                        result = last / 100;

                        operationLogBuilder.Append("%");
                        operationLogBuilder.Append("=");
                        operationLogBuilder.Append(result);
                        textViewOperationLog.Text = operationLogBuilder.ToString();

                        numbers[0] = result.ToString();
                        _operator = null;
                        numbers[1] = null;
                        UpdateCalculatorTextView();

                    }
                    //If 5+%
                    else if (numbers[1] == null)
                    {
                        double? last = numbers[0] == null ? null : (double?)double.Parse(numbers[0]);
                        result = last / 100;

                        operationLogBuilder.Append(last);
                        operationLogBuilder.Append("%");
                        operationLogBuilder.Append("=");
                        operationLogBuilder.Append(result);
                        textViewOperationLog.Text = operationLogBuilder.ToString();

                        numbers[0] = result.ToString();
                        _operator = null;
                        numbers[1] = null;
                        UpdateCalculatorTextView();

                    }
                }
                else
                {
                    return;
                }
            }
            else
            {
                return;
            }
        }

        private void RootOver()
        {
            if (numbers[0] == null && _operator == null && numbers[1] == null)
            {
                return;
            }
            else if (numbers[0] != null && _operator == null && numbers[1] == null)
            {
                double? first = numbers[0] == null ? null : (double?)double.Parse(numbers[0]);
                result = Math.Sqrt((double)first);

                //Empty the operationBuilderLog
                for (int i = 0; i < operationLogBuilder.Length; i++)
                {
                    operationLogBuilder[i] = '\0';
                }
                operationLogBuilder.Append("√");
                operationLogBuilder.Append(first);
                operationLogBuilder.Append("=");
                operationLogBuilder.Append(result);
                textViewOperationLog.Text = operationLogBuilder.ToString();

                numbers[0] = result.ToString();
                _operator = null;
                numbers[1] = null;
                UpdateCalculatorTextView();

            }
            else if (numbers[0] != null && _operator != null && numbers[1] == null)
            {
                double? first = numbers[0] == null ? null : (double?)double.Parse(numbers[0]);
                result = Math.Sqrt((double)first);

                //Empty the operationBuilderLog
                for (int i = 0; i < operationLogBuilder.Length; i++)
                {
                    operationLogBuilder[i] = '\0';
                }
                operationLogBuilder.Append("√");
                operationLogBuilder.Append(first);
                operationLogBuilder.Append("=");
                operationLogBuilder.Append(result);
                textViewOperationLog.Text = operationLogBuilder.ToString();

                numbers[0] = result.ToString();
                _operator = null;
                numbers[1] = null;
                UpdateCalculatorTextView();

            }
            else if (numbers[0] != null && _operator != null && numbers[1] != null)
            {
                double? first = numbers[0] == null ? null : (double?)double.Parse(numbers[0]);
                double? last = numbers[1] == null ? null : (double?)double.Parse(numbers[1]);

                switch (_operator)
                {
                    case "+":
                        result = first + Math.Sqrt((double)last);
                        break;
                    case "-":
                        result = first - Math.Sqrt((double)last);
                        break;
                    case "×":
                        result = first * Math.Sqrt((double)last);
                        break;
                    case "÷":
                        if (last != 0)
                        {
                            result = first / Math.Sqrt((double)last);
                        }
                        else
                        {
                            result = null;
                        }

                        break;
                }

                if (result != null)
                {
                    //Empty the last entry of operationBuilderLog
                    for (int i = (operationLogBuilder.Length - last.ToString().Length); i < operationLogBuilder.Length; i++)
                    {
                        operationLogBuilder[i] = '\0';
                    }
                    operationLogBuilder.Append("√");
                    operationLogBuilder.Append(last);
                    operationLogBuilder.Append("=");
                    operationLogBuilder.Append(result);
                    textViewOperationLog.Text = operationLogBuilder.ToString();

                    numbers[0] = result.ToString();
                    _operator = null;
                    numbers[1] = null;
                    UpdateCalculatorTextView();

                }
                else if (result == null)
                {
                    //Empty the last entry of operationBuilderLog
                    for (int i = (operationLogBuilder.Length - last.ToString().Length); i < operationLogBuilder.Length; i++)
                    {
                        operationLogBuilder[i] = '\0';
                    }
                    operationLogBuilder.Append("√");
                    operationLogBuilder.Append(last);
                    operationLogBuilder.Append("=");
                    operationLogBuilder.Append("null");
                    textViewOperationLog.Text = operationLogBuilder.ToString();

                    numbers[0] = null;
                    _operator = null;
                    numbers[1] = null;
                    textViewResult.Text = "Cannot divide by zero";
                }
            }
            else
            {
                return;
            }
        }

        private void SquareOver()
        {
            if (numbers[0] == null && _operator == null && numbers[1] == null)
            {
                return;
            }
            else if (numbers[0] != null && _operator == null && numbers[1] == null)
            {
                double? first = numbers[0] == null ? null : (double?)double.Parse(numbers[0]);
                result = first * first;

                //Empty the operationBuilderLog
                for (int i = 0; i < operationLogBuilder.Length; i++)
                {
                    operationLogBuilder[i] = '\0';
                }
                operationLogBuilder.Append("(");
                operationLogBuilder.Append(first);
                operationLogBuilder.Append(")²");
                operationLogBuilder.Append("=");
                operationLogBuilder.Append(result);
                textViewOperationLog.Text = operationLogBuilder.ToString();


                numbers[0] = result.ToString();
                _operator = null;
                numbers[1] = null;
                UpdateCalculatorTextView();

            }
            else if (numbers[0] != null && _operator != null && numbers[1] == null)
            {
                double? first = numbers[0] == null ? null : (double?)double.Parse(numbers[0]);
                result = first * first;

                //Empty the operationBuilderLog
                for (int i = 0; i < operationLogBuilder.Length; i++)
                {
                    operationLogBuilder[i] = '\0';
                }
                operationLogBuilder.Append("(");
                operationLogBuilder.Append(first);
                operationLogBuilder.Append(")²");
                operationLogBuilder.Append("=");
                operationLogBuilder.Append(result);
                textViewOperationLog.Text = operationLogBuilder.ToString();

                numbers[0] = result.ToString();
                _operator = null;
                numbers[1] = null;
                UpdateCalculatorTextView();

            }
            else if (numbers[0] != null && _operator != null && numbers[1] != null)
            {
                double? first = numbers[0] == null ? null : (double?)double.Parse(numbers[0]);
                double? last = numbers[1] == null ? null : (double?)double.Parse(numbers[1]);

                switch (_operator)
                {
                    case "+":
                        result = first + (last * last);
                        break;
                    case "-":
                        result = first - (last * last);
                        break;
                    case "×":
                        result = first * (last * last);
                        break;
                    case "÷":
                        if (last != 0)
                        {
                            result = first / (last * last);
                        }
                        else
                        {
                            result = null;
                        }

                        break;
                }

                if (result != null)
                {
                    //Empty the last entry of operationBuilderLog
                    for (int i = (operationLogBuilder.Length - last.ToString().Length); i < operationLogBuilder.Length; i++)
                    {
                        operationLogBuilder[i] = '\0';
                    }
                    operationLogBuilder.Append("(");
                    operationLogBuilder.Append(last);
                    operationLogBuilder.Append(")²");
                    operationLogBuilder.Append("=");
                    operationLogBuilder.Append(result);
                    textViewOperationLog.Text = operationLogBuilder.ToString();

                    numbers[0] = result.ToString();
                    _operator = null;
                    numbers[1] = null;
                    UpdateCalculatorTextView();

                }
                else if (result == null)
                {
                    //Empty the last entry of operationBuilderLog
                    for (int i = (operationLogBuilder.Length - last.ToString().Length); i < operationLogBuilder.Length; i++)
                    {
                        operationLogBuilder[i] = '\0';
                    }
                    operationLogBuilder.Append("(");
                    operationLogBuilder.Append(last);
                    operationLogBuilder.Append(")²");
                    operationLogBuilder.Append("=");
                    operationLogBuilder.Append("null");
                    textViewOperationLog.Text = operationLogBuilder.ToString();


                    numbers[0] = null;
                    _operator = null;
                    numbers[1] = null;
                    textViewResult.Text = "Cannot divide by zero";
                }
            }
            else
            {
                return;
            }
        }

        private void DividedByOne()
        {
            if (numbers[0] == null && _operator == null && numbers[1] == null)
            {
                return;
            }
            else if (numbers[0] != null && _operator == null && numbers[1] == null)
            {
                double? first = numbers[0] == null ? null : (double?)double.Parse(numbers[0]);
                result = 1 / first;

                //Empty the operationBuilderLog
                for (int i = 0; i < operationLogBuilder.Length; i++)
                {
                    operationLogBuilder[i] = '\0';
                }
                operationLogBuilder.Append("1");
                operationLogBuilder.Append("÷");
                operationLogBuilder.Append(first);
                operationLogBuilder.Append("=");
                operationLogBuilder.Append(result);
                textViewOperationLog.Text = operationLogBuilder.ToString();

                numbers[0] = result.ToString();
                _operator = null;
                numbers[1] = null;
                UpdateCalculatorTextView();

            }
            else if (numbers[0] != null && _operator != null && numbers[1] == null)
            {
                double? first = numbers[0] == null ? null : (double?)double.Parse(numbers[0]);
                result = 1 / first;

                //Empty the operationBuilderLog
                for (int i = 0; i < operationLogBuilder.Length; i++)
                {
                    operationLogBuilder[i] = '\0';
                }
                operationLogBuilder.Append("1");
                operationLogBuilder.Append("÷");
                operationLogBuilder.Append(first);
                operationLogBuilder.Append("=");
                operationLogBuilder.Append(result);
                textViewOperationLog.Text = operationLogBuilder.ToString();

                numbers[0] = result.ToString();
                _operator = null;
                numbers[1] = null;
                UpdateCalculatorTextView();

            }
            else if (numbers[0] != null && _operator != null && numbers[1] != null)
            {
                double? first = numbers[0] == null ? null : (double?)double.Parse(numbers[0]);
                double? last = numbers[1] == null ? null : (double?)double.Parse(numbers[1]);

                switch (_operator)
                {
                    case "+":
                        result = first + (1 / last);
                        break;
                    case "-":
                        result = first - (1 / last);
                        break;
                    case "×":
                        result = first * (1 / last);
                        break;
                    case "÷":
                        if (last != 0)
                        {
                            result = first / (1 / last);
                        }
                        else
                        {
                            result = null;
                        }
                        break;
                }

                if (result != null)
                {
                    //Empty the last entry of operationBuilderLog
                    for (int i = (operationLogBuilder.Length - last.ToString().Length); i < operationLogBuilder.Length; i++)
                    {
                        operationLogBuilder[i] = '\0';
                    }
                    operationLogBuilder.Append("(");
                    operationLogBuilder.Append("1");
                    operationLogBuilder.Append("÷");
                    operationLogBuilder.Append(last);
                    operationLogBuilder.Append(")");
                    operationLogBuilder.Append("=");
                    operationLogBuilder.Append(result);
                    textViewOperationLog.Text = operationLogBuilder.ToString();


                    numbers[0] = result.ToString();
                    _operator = null;
                    numbers[1] = null;
                    UpdateCalculatorTextView();

                }
                else if (result == null)
                {
                    //Empty the last entry of operationBuilderLog
                    for (int i = (operationLogBuilder.Length - last.ToString().Length); i < operationLogBuilder.Length; i++)
                    {
                        operationLogBuilder[i] = '\0';
                    }
                    operationLogBuilder.Append("(");
                    operationLogBuilder.Append("1");
                    operationLogBuilder.Append("÷");
                    operationLogBuilder.Append(last);
                    operationLogBuilder.Append(")");
                    operationLogBuilder.Append("=");
                    operationLogBuilder.Append("null");
                    textViewOperationLog.Text = operationLogBuilder.ToString();


                    numbers[0] = null;
                    _operator = null;
                    numbers[1] = null;
                    textViewResult.Text = "Cannot divide by zero";
                }
            }
            else
            {
                return;
            }
        }
    }
}