# prepare.py
# Converts MNIST-formatted files at the passed-in input path to a passed-in output path
import os
import sys

# Conversion routine for MNIST binary format
def convert(imgf, labelf, outf, n):
    f = open(imgf, "rb")
    l = open(labelf, "rb")
    o = open(outf, "w")

    f.read(16)
    l.read(8)
    images = []

    for i in range(n):
        image = [ord(l.read(1))]
        for j in range(28 * 28):
            image.append(ord(f.read(1)))
        images.append(image)

    for image in images:
        o.write(",".join(str(pix) for pix in image) + "\n")
    f.close()
    o.close()
    l.close()

# The MNIST-formatted source
mounted_input_path = sys.argv[1]
# The output directory at which the outputs will be written
mounted_output_path = sys.argv[2]

# Create the output directory
os.makedirs(mounted_output_path, exist_ok=True)

# Convert the training data
convert(
    os.path.join(mounted_input_path, "mnist-fashion/train-images-idx3-ubyte"),
    os.path.join(mounted_input_path, "mnist-fashion/train-labels-idx1-ubyte"),
    os.path.join(mounted_output_path, "mnist_train.csv"),
    60000,
)

# Convert the test data
convert(
    os.path.join(mounted_input_path, "mnist-fashion/t10k-images-idx3-ubyte"),
    os.path.join(mounted_input_path, "mnist-fashion/t10k-labels-idx1-ubyte"),
    os.path.join(mounted_output_path, "mnist_test.csv"),
    10000,
)