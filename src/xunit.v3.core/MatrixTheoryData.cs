using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit;

/// <summary>
/// Represents theory data which is created from the merging of two data streams by
/// creating a matrix of the data.
/// </summary>
/// <typeparam name="T1">Type of the first data dimension</typeparam>
/// <typeparam name="T2">Type of the second data dimension</typeparam>
public class MatrixTheoryData<T1, T2> : TheoryData<T1, T2>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="MatrixTheoryData{T1, T2}"/> class.
	/// </summary>
	/// <param name="dimension1">Data for the first dimension</param>
	/// <param name="dimension2">Data for the second dimension</param>
	public MatrixTheoryData(
		IEnumerable<T1> dimension1,
		IEnumerable<T2> dimension2)
	{
		Guard.ArgumentNotNull(dimension1);
		Guard.ArgumentNotNull(dimension2);

		var data1Empty = true;
		var data2Empty = true;

		foreach (var t1 in dimension1)
		{
			data1Empty = false;

			foreach (var t2 in dimension2)
			{
				data2Empty = false;
				Add(t1, t2);
			}
		}

		Guard.ArgumentValid("Data dimension cannot be empty", !data1Empty, nameof(dimension1));
		Guard.ArgumentValid("Data dimension cannot be empty", !data2Empty, nameof(dimension2));
	}
}

/// <summary>
/// Represents theory data which is created from the merging of three data streams by
/// creating a matrix of the data.
/// </summary>
/// <typeparam name="T1">Type of the first data dimension</typeparam>
/// <typeparam name="T2">Type of the second data dimension</typeparam>
/// <typeparam name="T3">Type of the third data dimension</typeparam>
public class MatrixTheoryData<T1, T2, T3> : TheoryData<T1, T2, T3>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="MatrixTheoryData{T1, T2, T3}"/> class.
	/// </summary>
	/// <param name="dimension1">Data for the first dimension</param>
	/// <param name="dimension2">Data for the second dimension</param>
	/// <param name="dimension3">Data for the third dimension</param>
	public MatrixTheoryData(
		IEnumerable<T1> dimension1,
		IEnumerable<T2> dimension2,
		IEnumerable<T3> dimension3)
	{
		Guard.ArgumentNotNull(dimension1);
		Guard.ArgumentNotNull(dimension2);
		Guard.ArgumentNotNull(dimension3);

		var data1Empty = true;
		var data2Empty = true;
		var data3Empty = true;

		foreach (var t1 in dimension1)
		{
			data1Empty = false;

			foreach (var t2 in dimension2)
			{
				data2Empty = false;

				foreach (var t3 in dimension3)
				{
					data3Empty = false;
					Add(t1, t2, t3);
				}
			}
		}

		Guard.ArgumentValid("Data dimension cannot be empty", !data1Empty, nameof(dimension1));
		Guard.ArgumentValid("Data dimension cannot be empty", !data2Empty, nameof(dimension2));
		Guard.ArgumentValid("Data dimension cannot be empty", !data3Empty, nameof(dimension3));
	}
}

/// <summary>
/// Represents theory data which is created from the merging of four data streams by
/// creating a matrix of the data.
/// </summary>
/// <typeparam name="T1">Type of the first data dimension</typeparam>
/// <typeparam name="T2">Type of the second data dimension</typeparam>
/// <typeparam name="T3">Type of the third data dimension</typeparam>
/// <typeparam name="T4">Type of the fourth data dimension</typeparam>
public class MatrixTheoryData<T1, T2, T3, T4> : TheoryData<T1, T2, T3, T4>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="MatrixTheoryData{T1, T2, T3, T4}"/> class.
	/// </summary>
	/// <param name="dimension1">Data for the first dimension</param>
	/// <param name="dimension2">Data for the second dimension</param>
	/// <param name="dimension3">Data for the third dimension</param>
	/// <param name="dimension4">Data for the fourth dimension</param>
	public MatrixTheoryData(
		IEnumerable<T1> dimension1,
		IEnumerable<T2> dimension2,
		IEnumerable<T3> dimension3,
		IEnumerable<T4> dimension4)
	{
		Guard.ArgumentNotNull(dimension1);
		Guard.ArgumentNotNull(dimension2);
		Guard.ArgumentNotNull(dimension3);
		Guard.ArgumentNotNull(dimension4);

		var data1Empty = true;
		var data2Empty = true;
		var data3Empty = true;
		var data4Empty = true;

		foreach (var t1 in dimension1)
		{
			data1Empty = false;

			foreach (var t2 in dimension2)
			{
				data2Empty = false;

				foreach (var t3 in dimension3)
				{
					data3Empty = false;

					foreach (var t4 in dimension4)
					{
						data4Empty = false;
						Add(t1, t2, t3, t4);
					}
				}
			}
		}

		Guard.ArgumentValid("Data dimension cannot be empty", !data1Empty, nameof(dimension1));
		Guard.ArgumentValid("Data dimension cannot be empty", !data2Empty, nameof(dimension2));
		Guard.ArgumentValid("Data dimension cannot be empty", !data3Empty, nameof(dimension3));
		Guard.ArgumentValid("Data dimension cannot be empty", !data4Empty, nameof(dimension4));
	}
}

/// <summary>
/// Represents theory data which is created from the merging of five data streams by
/// creating a matrix of the data.
/// </summary>
/// <typeparam name="T1">Type of the first data dimension</typeparam>
/// <typeparam name="T2">Type of the second data dimension</typeparam>
/// <typeparam name="T3">Type of the third data dimension</typeparam>
/// <typeparam name="T4">Type of the fourth data dimension</typeparam>
/// <typeparam name="T5">Type of the fifth data dimension</typeparam>
public class MatrixTheoryData<T1, T2, T3, T4, T5> : TheoryData<T1, T2, T3, T4, T5>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="MatrixTheoryData{T1, T2, T3, T4, T5}"/> class.
	/// </summary>
	/// <param name="dimension1">Data for the first dimension</param>
	/// <param name="dimension2">Data for the second dimension</param>
	/// <param name="dimension3">Data for the third dimension</param>
	/// <param name="dimension4">Data for the fourth dimension</param>
	/// <param name="dimension5">Data for the fifth dimension</param>
	public MatrixTheoryData(
		IEnumerable<T1> dimension1,
		IEnumerable<T2> dimension2,
		IEnumerable<T3> dimension3,
		IEnumerable<T4> dimension4,
		IEnumerable<T5> dimension5)
	{
		Guard.ArgumentNotNull(dimension1);
		Guard.ArgumentNotNull(dimension2);
		Guard.ArgumentNotNull(dimension3);
		Guard.ArgumentNotNull(dimension4);
		Guard.ArgumentNotNull(dimension5);

		var data1Empty = true;
		var data2Empty = true;
		var data3Empty = true;
		var data4Empty = true;
		var data5Empty = true;

		foreach (var t1 in dimension1)
		{
			data1Empty = false;

			foreach (var t2 in dimension2)
			{
				data2Empty = false;

				foreach (var t3 in dimension3)
				{
					data3Empty = false;

					foreach (var t4 in dimension4)
					{
						data4Empty = false;

						foreach (var t5 in dimension5)
						{
							data5Empty = false;
							Add(t1, t2, t3, t4, t5);
						}
					}
				}
			}
		}

		Guard.ArgumentValid("Data dimension cannot be empty", !data1Empty, nameof(dimension1));
		Guard.ArgumentValid("Data dimension cannot be empty", !data2Empty, nameof(dimension2));
		Guard.ArgumentValid("Data dimension cannot be empty", !data3Empty, nameof(dimension3));
		Guard.ArgumentValid("Data dimension cannot be empty", !data4Empty, nameof(dimension4));
		Guard.ArgumentValid("Data dimension cannot be empty", !data5Empty, nameof(dimension5));
	}
}
