#include "Draw.h"
#include <algorithm>
#include <queue>
#include <string>

extern "C" {

	void SeedRandomization()
	{
		srand(0);
	}

	void FillWithColor(Texture Image, int x, int y,const Color ReplacementColor)
	{
		Color TargetColor = Image.GetColor(x, y);
		if (TargetColor.CheckEqual(ReplacementColor))
			return;

		FMask mask(Image.Width, Image.Height);

		std::queue<Node> Q;
		Q.push(Node(x, y, Image.Width));

		while (!Q.empty())
		{
			Node node = Q.front();
			Q.pop();

			// fill the right half of the paper
			for (int i = node.x; i < Image.Width; i++)
			{
				Color c = Image.GetColor(i, node.y);
				if (!c.CheckEqual(TargetColor) || mask.data[node.y][i])
					break;

				mask.data[node.y][i] = 1;
				if (node.y + 1 < Image.Height)
				{
					c = Image.GetColor(i + Image.Width, node.y);
					if (c.CheckEqual(TargetColor) && !mask.data[node.y + 1][i])
						Q.push(Node(i, node.y + 1, Image.Width));
				}
				if (node.y - 1 >= 0)
				{
					c = Image.GetColor(i - Image.Width, node.y);
					if (c.CheckEqual(TargetColor) && !mask.data[node.y - 1][i])
						Q.push(Node(i, node.y - 1, Image.Width));
				}
			}

			// fill left half of the paper
			for (int i = node.x - 1; i >= 0; i--)
			{
				Color c = Image.GetColor(i, node.y);
				if (!c.CheckEqual(TargetColor) || mask.data[node.y][i])
					break;

				mask.data[node.y][i] = 1;
				if (node.y + 1 < Image.Height)
				{
					c = Image.GetColor(i + Image.Width, node.y);
					if (c.CheckEqual(TargetColor) && !mask.data[node.y + 1][i])
						Q.push(Node(i, node.y + 1, Image.Width));
				}
				if (node.y - 1 >= 0)
				{
					c = Image.GetColor(i - Image.Width, node.y);
					if (c.CheckEqual(TargetColor) && !mask.data[node.y - 1][i])
						Q.push(Node(i, node.y - 1, Image.Width));
				}
			}
		}

		Image.SetColorWithMask(mask, ReplacementColor);
	}

	void FillWithTexture(Texture Image, int x, int y, Texture PatternTexture)
	{
		Color TargetColor = Image.GetColor(x, y);

		FMask mask(Image.Width, Image.Height);

		std::queue<Node> Q;
		Q.push(Node(x, y, Image.Width));

		while (!Q.empty())
		{
			Node node = Q.front();
			Q.pop();

			// fill the right half of the paper
			for (int i = node.x; i < Image.Width; i++)
			{
				Color c = Image.GetColor(i, node.y);
				if (!c.CheckEqual(TargetColor) || mask.data[node.y][i])
					break;

				mask.data[node.y][i] = 1;
				if (node.y + 1 < Image.Height)
				{
					c = Image.GetColor(i + Image.Width, node.y);
					if (c.CheckEqual(TargetColor) && !mask.data[node.y + 1][i])
						Q.push(Node(i, node.y + 1, Image.Width));
				}
				if (node.y - 1 >= 0)
				{
					c = Image.GetColor(i - Image.Width, node.y);
					if (c.CheckEqual(TargetColor) && !mask.data[node.y - 1][i])
						Q.push(Node(i, node.y - 1, Image.Width));
				}
			}

			// fill left half of the paper
			for (int i = node.x - 1; i >= 0; i--)
			{
				Color c = Image.GetColor(i, node.y);
				if (!c.CheckEqual(TargetColor) || mask.data[node.y][i])
					break;

				mask.data[node.y][i] = 1;
				if (node.y + 1 < Image.Height)
				{
					c = Image.GetColor(i + Image.Width, node.y);
					if (c.CheckEqual(TargetColor) && !mask.data[node.y + 1][i])
						Q.push(Node(i, node.y + 1, Image.Width));
				}
				if (node.y - 1 >= 0)
				{
					c = Image.GetColor(i - Image.Width, node.y);
					if (c.CheckEqual(TargetColor) && !mask.data[node.y - 1][i])
						Q.push(Node(i, node.y - 1, Image.Width));
				}
			}
		}

		Image.SetColorWithMask(mask, PatternTexture);
	}

	void GetBrightTexture(Texture Image, Color MainColor)
	{
		Color tempColor;
		float ratio;
		int HalfWidth = Image.Width / 2;

		for (int i = 0; i < HalfWidth + 1; i++)
		{
			ratio = (float)i / (float)HalfWidth;
			tempColor = Color::Lerp(Black, MainColor, ratio);
			for (int j = 0; j < Image.Height; j++)
			{
				Image.SetColor(i, j, tempColor);
			}
		}

		for (int i = 0; i < HalfWidth; i++)
		{
			ratio = (float)i / (float)HalfWidth;
			tempColor = Color::Lerp(MainColor, White, ratio);
			for (int j = 0; j < Image.Height; j++)
			{
				Image.SetColor(i + HalfWidth, j, tempColor);
			}
		}
	}

	void DrawBrushTip(Texture Image, Brush BrushData, Color DrawColor, int x, int y)
	{
		Image.DrawBrushTip(BrushData, DrawColor, x, y);
	}

	void DrawBrushTipWithTex(Texture Image, Brush BrushData, Texture DrawColor, int x, int y)
	{
		Image.DrawBrushTipWithTexture(BrushData, DrawColor, x, y);
	}

	void DrawSprayWithBrush(Texture Image, Brush BrushData, Color DrawColor, int x, int y)
	{
		Image.DrawSprayWithBrush(BrushData, DrawColor, x, y);
	}

	void DrawLine(Texture Image, Brush BrushData, Color DrawColor, int x0, int y0, int x1, int y1, Vector* FinalPos)
	{
		int SpacingY = (int)(BrushData.Spacing * BrushData.Direction.y);
		int SpacingX = (int)(BrushData.Spacing * BrushData.Direction.x);


		if (abs(y1 - y0) < abs(x1 - x0))
		{
			while (abs(SpacingX) < abs(x1 - x0))
			{
				x0 += SpacingX;
				y0 += SpacingY;

				Image.DrawBrushTip(BrushData, DrawColor, x0, y0);
			}
		}
		else
		{
			while (abs(SpacingY) < abs(y1 - y0))
			{
				x0 += SpacingX;
				y0 += SpacingY;

				Image.DrawBrushTip(BrushData, DrawColor, x0, y0);
			}
		}

		FinalPos->x = (float)x0;
		FinalPos->y = (float)y0;
	}

	void DrawLineWithTex(Texture Image, Brush BrushData, Texture DrawColor, int x0, int y0, int x1, int y1, Vector* FinalPos)
	{
		int SpacingY = (int)(BrushData.Spacing * BrushData.Direction.y);
		int SpacingX = (int)(BrushData.Spacing * BrushData.Direction.x);

		if (abs(y1 - y0) < abs(x1 - x0))
		{
			while (abs(SpacingX) < abs(x1 - x0))
			{
				x0 += SpacingX;
				y0 += SpacingY;

				Image.DrawBrushTipWithTexture(BrushData, DrawColor, x0, y0);
			}
		}
		else
		{
			while (abs(SpacingY) < abs(y1 - y0))
			{
				x0 += SpacingX;
				y0 += SpacingY;

				Image.DrawBrushTipWithTexture(BrushData, DrawColor, x0, y0);
			}
		}

		FinalPos->x = (float)x0;
		FinalPos->y = (float)y0;
	}

	void RegisterDebugCallback(DebugCallback callback)
	{
		if (callback)
		{
			gDebugCallback = callback;
		}
	}
}

int GetRandomBoolean()
{
	return rand() % 10;
}

static void Print(std::string message)
{
	gDebugCallback(message.c_str());
}

Vector CatmullRom(Vector p0, Vector p1, Vector p2, Vector p3, float i)
{
	// comments are no use here... it's the catmull-rom equation.
	// Un-magic this, lord vector!
	return 0.5f*
		((2.0f * p1) + (-p0 + p2)*i + (2 * p0 - 5 * p1 + 4 * p2 - p3)*i*i +
		(-p0 + 3 * p1 - 3 * p2 + p3)*i*i*i);
}

int Clip(int n, int lower, int upper) {
	return std::max(lower, std::min(n, upper));
}