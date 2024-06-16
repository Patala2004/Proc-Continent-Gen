from PIL import Image

# Define constants
BLOCK_SIZE = 16
WIDTH = BLOCK_SIZE * 255
HEIGHT = BLOCK_SIZE

# Create a new image with the specified dimensions
image = Image.new('L', (WIDTH, HEIGHT))

# Iterate over each 16x16 block
for x in range(0, WIDTH, BLOCK_SIZE):
    for y in range(0, HEIGHT, BLOCK_SIZE):
        # Calculate grayscale value based on x position
        gray_value = int(x / WIDTH * 255)
        
        # Fill the block with the calculated grayscale value
        for i in range(BLOCK_SIZE):
            for j in range(BLOCK_SIZE):
                image.putpixel((x + i, y + j), gray_value)

# Save the image as grayscale.png
image.save('grayscale.png')

print('Image saved as grayscale.png')
