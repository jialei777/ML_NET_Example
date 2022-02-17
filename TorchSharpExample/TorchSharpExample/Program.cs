using System.Runtime.InteropServices;
using TorchSharp;
using static TorchSharp.torch.nn;

var lin1 = Linear(10, 100);
var lin2 = Linear(100, 10);
var seq = Sequential(("lin1", lin1), ("relu1", ReLU()), ("drop1", Dropout(0.1)), ("lin2", lin2));


var x_test = torch.randn(64, 10);
var y_true = x_test.clone();

var optimizer = torch.optim.Adam(seq.parameters());
var loss = functional.mse_loss(Reduction.Sum);

seq.train();
for (int i = 0; i < 501; i++)
{
    var x = torch.randn(64, 10);
    var y = x.clone();

    var eval = seq.forward(x);
    var output = loss(eval, y);

    if ( i % 50 == 0)
    {
        seq.eval();
        var y_pred = seq.forward(x_test);
        var test_loss = loss(y_pred, y_true);
        Console.WriteLine($"Iteration {i}: training loss is {output.item<float>()}, test MSE is {test_loss.item<float>()}");

        seq.train();
    }

    optimizer.zero_grad();
    output.backward();
    optimizer.step();
}


seq.eval();
var y_pred_final = seq.forward(x_test);
var test_loss_final = loss(y_pred_final, y_true);
Console.WriteLine($"Finial test MSE is {test_loss_final.item<float>()}");
Console.WriteLine("Now save the model...");
seq.save("..//..//..//model.dat");


var new_model = Sequential(("lin1", lin1), ("relu1", ReLU()), ("drop1", Dropout(0.1)), ("lin2", lin2));
new_model.load("..//..//..//model.dat");
new_model.eval();
var y_new = new_model.forward(x_test);
var test_loss_new = loss(y_new, y_true);
Console.WriteLine($"Test MSE from the loaded model is {test_loss_new.item<float>()}");

