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

/// <summary>
/// Represents theory data which is created from the merging of six data streams by
/// creating a matrix of the data.
/// </summary>
/// <typeparam name="T1">Type of the first data dimension</typeparam>
/// <typeparam name="T2">Type of the second data dimension</typeparam>
/// <typeparam name="T3">Type of the third data dimension</typeparam>
/// <typeparam name="T4">Type of the fourth data dimension</typeparam>
/// <typeparam name="T5">Type of the fifth data dimension</typeparam>
/// <typeparam name="T6">Type of the sixth data dimension</typeparam>
public class MatrixTheoryData<T1, T2, T3, T4, T5, T6> : TheoryData<T1, T2, T3, T4, T5, T6>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="MatrixTheoryData{T1, T2, T3, T4, T5, T6}"/> class.
	/// </summary>
	/// <param name="dimension1">Data for the first dimension</param>
	/// <param name="dimension2">Data for the second dimension</param>
	/// <param name="dimension3">Data for the third dimension</param>
	/// <param name="dimension4">Data for the fourth dimension</param>
	/// <param name="dimension5">Data for the fifth dimension</param>
	/// <param name="dimension6">Data for the sixth dimension</param>
	public MatrixTheoryData(
		IEnumerable<T1> dimension1,
		IEnumerable<T2> dimension2,
		IEnumerable<T3> dimension3,
		IEnumerable<T4> dimension4,
		IEnumerable<T5> dimension5,
		IEnumerable<T6> dimension6)
	{
		Guard.ArgumentNotNull(dimension1);
		Guard.ArgumentNotNull(dimension2);
		Guard.ArgumentNotNull(dimension3);
		Guard.ArgumentNotNull(dimension4);
		Guard.ArgumentNotNull(dimension5);
		Guard.ArgumentNotNull(dimension6);

		var data1Empty = true;
		var data2Empty = true;
		var data3Empty = true;
		var data4Empty = true;
		var data5Empty = true;
		var data6Empty = true;

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

							foreach (var t6 in dimension6)
							{
								data6Empty = false;

								Add(t1, t2, t3, t4, t5, t6);
							}
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
		Guard.ArgumentValid("Data dimension cannot be empty", !data6Empty, nameof(dimension6));
	}
}

/// <summary>
/// Represents theory data which is created from the merging of seven data streams by
/// creating a matrix of the data.
/// </summary>
/// <typeparam name="T1">Type of the first data dimension</typeparam>
/// <typeparam name="T2">Type of the second data dimension</typeparam>
/// <typeparam name="T3">Type of the third data dimension</typeparam>
/// <typeparam name="T4">Type of the fourth data dimension</typeparam>
/// <typeparam name="T5">Type of the fifth data dimension</typeparam>
/// <typeparam name="T6">Type of the sixth data dimension</typeparam>
/// <typeparam name="T7">Type of the seventh data dimension</typeparam>
public class MatrixTheoryData<T1, T2, T3, T4, T5, T6, T7> : TheoryData<T1, T2, T3, T4, T5, T6, T7>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="MatrixTheoryData{T1, T2, T3, T4, T5, T6, T7}"/> class.
	/// </summary>
	/// <param name="dimension1">Data for the first dimension</param>
	/// <param name="dimension2">Data for the second dimension</param>
	/// <param name="dimension3">Data for the third dimension</param>
	/// <param name="dimension4">Data for the fourth dimension</param>
	/// <param name="dimension5">Data for the fifth dimension</param>
	/// <param name="dimension6">Data for the sixth dimension</param>
	/// <param name="dimension7">Data for the seventh dimension</param>
	public MatrixTheoryData(
		IEnumerable<T1> dimension1,
		IEnumerable<T2> dimension2,
		IEnumerable<T3> dimension3,
		IEnumerable<T4> dimension4,
		IEnumerable<T5> dimension5,
		IEnumerable<T6> dimension6,
		IEnumerable<T7> dimension7)
	{
		Guard.ArgumentNotNull(dimension1);
		Guard.ArgumentNotNull(dimension2);
		Guard.ArgumentNotNull(dimension3);
		Guard.ArgumentNotNull(dimension4);
		Guard.ArgumentNotNull(dimension5);
		Guard.ArgumentNotNull(dimension6);
		Guard.ArgumentNotNull(dimension7);

		var data1Empty = true;
		var data2Empty = true;
		var data3Empty = true;
		var data4Empty = true;
		var data5Empty = true;
		var data6Empty = true;
		var data7Empty = true;

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

							foreach (var t6 in dimension6)
							{
								data6Empty = false;

								foreach (var t7 in dimension7)
								{
									data7Empty = false;

									Add(t1, t2, t3, t4, t5, t6, t7);
								}
							}
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
		Guard.ArgumentValid("Data dimension cannot be empty", !data6Empty, nameof(dimension6));
		Guard.ArgumentValid("Data dimension cannot be empty", !data7Empty, nameof(dimension7));
	}
}

/// <summary>
/// Represents theory data which is created from the merging of eight data streams by
/// creating a matrix of the data.
/// </summary>
/// <typeparam name="T1">Type of the first data dimension</typeparam>
/// <typeparam name="T2">Type of the second data dimension</typeparam>
/// <typeparam name="T3">Type of the third data dimension</typeparam>
/// <typeparam name="T4">Type of the fourth data dimension</typeparam>
/// <typeparam name="T5">Type of the fifth data dimension</typeparam>
/// <typeparam name="T6">Type of the sixth data dimension</typeparam>
/// <typeparam name="T7">Type of the seventh data dimension</typeparam>
/// <typeparam name="T8">Type of the eighth data dimension</typeparam>
public class MatrixTheoryData<T1, T2, T3, T4, T5, T6, T7, T8> : TheoryData<T1, T2, T3, T4, T5, T6, T7, T8>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="MatrixTheoryData{T1, T2, T3, T4, T5, T6, T7, T8}"/> class.
	/// </summary>
	/// <param name="dimension1">Data for the first dimension</param>
	/// <param name="dimension2">Data for the second dimension</param>
	/// <param name="dimension3">Data for the third dimension</param>
	/// <param name="dimension4">Data for the fourth dimension</param>
	/// <param name="dimension5">Data for the fifth dimension</param>
	/// <param name="dimension6">Data for the sixth dimension</param>
	/// <param name="dimension7">Data for the seventh dimension</param>
	/// <param name="dimension8">Data for the eighth dimension</param>
	public MatrixTheoryData(
		IEnumerable<T1> dimension1,
		IEnumerable<T2> dimension2,
		IEnumerable<T3> dimension3,
		IEnumerable<T4> dimension4,
		IEnumerable<T5> dimension5,
		IEnumerable<T6> dimension6,
		IEnumerable<T7> dimension7,
		IEnumerable<T8> dimension8)
	{
		Guard.ArgumentNotNull(dimension1);
		Guard.ArgumentNotNull(dimension2);
		Guard.ArgumentNotNull(dimension3);
		Guard.ArgumentNotNull(dimension4);
		Guard.ArgumentNotNull(dimension5);
		Guard.ArgumentNotNull(dimension6);
		Guard.ArgumentNotNull(dimension7);
		Guard.ArgumentNotNull(dimension8);

		var data1Empty = true;
		var data2Empty = true;
		var data3Empty = true;
		var data4Empty = true;
		var data5Empty = true;
		var data6Empty = true;
		var data7Empty = true;
		var data8Empty = true;

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

							foreach (var t6 in dimension6)
							{
								data6Empty = false;

								foreach (var t7 in dimension7)
								{
									data7Empty = false;

									foreach (var t8 in dimension8)
									{
										data8Empty = false;

										Add(t1, t2, t3, t4, t5, t6, t7, t8);
									}
								}
							}
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
		Guard.ArgumentValid("Data dimension cannot be empty", !data6Empty, nameof(dimension6));
		Guard.ArgumentValid("Data dimension cannot be empty", !data7Empty, nameof(dimension7));
		Guard.ArgumentValid("Data dimension cannot be empty", !data8Empty, nameof(dimension8));
	}
}

/// <summary>
/// Represents theory data which is created from the merging of nine data streams by
/// creating a matrix of the data.
/// </summary>
/// <typeparam name="T1">Type of the first data dimension</typeparam>
/// <typeparam name="T2">Type of the second data dimension</typeparam>
/// <typeparam name="T3">Type of the third data dimension</typeparam>
/// <typeparam name="T4">Type of the fourth data dimension</typeparam>
/// <typeparam name="T5">Type of the fifth data dimension</typeparam>
/// <typeparam name="T6">Type of the sixth data dimension</typeparam>
/// <typeparam name="T7">Type of the seventh data dimension</typeparam>
/// <typeparam name="T8">Type of the eighth data dimension</typeparam>
/// <typeparam name="T9">Type of the ninth data dimension</typeparam>
public class MatrixTheoryData<T1, T2, T3, T4, T5, T6, T7, T8, T9> : TheoryData<T1, T2, T3, T4, T5, T6, T7, T8, T9>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="MatrixTheoryData{T1, T2, T3, T4, T5, T6, T7, T8, T9}"/> class.
	/// </summary>
	/// <param name="dimension1">Data for the first dimension</param>
	/// <param name="dimension2">Data for the second dimension</param>
	/// <param name="dimension3">Data for the third dimension</param>
	/// <param name="dimension4">Data for the fourth dimension</param>
	/// <param name="dimension5">Data for the fifth dimension</param>
	/// <param name="dimension6">Data for the sixth dimension</param>
	/// <param name="dimension7">Data for the seventh dimension</param>
	/// <param name="dimension8">Data for the eighth dimension</param>
	/// <param name="dimension9">Data for the ninth dimension</param>
	public MatrixTheoryData(
		IEnumerable<T1> dimension1,
		IEnumerable<T2> dimension2,
		IEnumerable<T3> dimension3,
		IEnumerable<T4> dimension4,
		IEnumerable<T5> dimension5,
		IEnumerable<T6> dimension6,
		IEnumerable<T7> dimension7,
		IEnumerable<T8> dimension8,
		IEnumerable<T9> dimension9)
	{
		Guard.ArgumentNotNull(dimension1);
		Guard.ArgumentNotNull(dimension2);
		Guard.ArgumentNotNull(dimension3);
		Guard.ArgumentNotNull(dimension4);
		Guard.ArgumentNotNull(dimension5);
		Guard.ArgumentNotNull(dimension6);
		Guard.ArgumentNotNull(dimension7);
		Guard.ArgumentNotNull(dimension8);
		Guard.ArgumentNotNull(dimension9);

		var data1Empty = true;
		var data2Empty = true;
		var data3Empty = true;
		var data4Empty = true;
		var data5Empty = true;
		var data6Empty = true;
		var data7Empty = true;
		var data8Empty = true;
		var data9Empty = true;

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

							foreach (var t6 in dimension6)
							{
								data6Empty = false;

								foreach (var t7 in dimension7)
								{
									data7Empty = false;

									foreach (var t8 in dimension8)
									{
										data8Empty = false;

										foreach (var t9 in dimension9)
										{
											data9Empty = false;

											Add(t1, t2, t3, t4, t5, t6, t7, t8, t9);
										}
									}
								}
							}
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
		Guard.ArgumentValid("Data dimension cannot be empty", !data6Empty, nameof(dimension6));
		Guard.ArgumentValid("Data dimension cannot be empty", !data7Empty, nameof(dimension7));
		Guard.ArgumentValid("Data dimension cannot be empty", !data8Empty, nameof(dimension8));
		Guard.ArgumentValid("Data dimension cannot be empty", !data9Empty, nameof(dimension9));
	}
}

/// <summary>
/// Represents theory data which is created from the merging of ten data streams by
/// creating a matrix of the data.
/// </summary>
/// <typeparam name="T1">Type of the first data dimension</typeparam>
/// <typeparam name="T2">Type of the second data dimension</typeparam>
/// <typeparam name="T3">Type of the third data dimension</typeparam>
/// <typeparam name="T4">Type of the fourth data dimension</typeparam>
/// <typeparam name="T5">Type of the fifth data dimension</typeparam>
/// <typeparam name="T6">Type of the sixth data dimension</typeparam>
/// <typeparam name="T7">Type of the seventh data dimension</typeparam>
/// <typeparam name="T8">Type of the eighth data dimension</typeparam>
/// <typeparam name="T9">Type of the ninth data dimension</typeparam>
/// <typeparam name="T10">Type of the tenth data dimension</typeparam>
public class MatrixTheoryData<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : TheoryData<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="MatrixTheoryData{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10}"/> class.
	/// </summary>
	/// <param name="dimension1">Data for the first dimension</param>
	/// <param name="dimension2">Data for the second dimension</param>
	/// <param name="dimension3">Data for the third dimension</param>
	/// <param name="dimension4">Data for the fourth dimension</param>
	/// <param name="dimension5">Data for the fifth dimension</param>
	/// <param name="dimension6">Data for the sixth dimension</param>
	/// <param name="dimension7">Data for the seventh dimension</param>
	/// <param name="dimension8">Data for the eighth dimension</param>
	/// <param name="dimension9">Data for the ninth dimension</param>
	/// <param name="dimension10">Data for the tenth dimension</param>
	public MatrixTheoryData(
		IEnumerable<T1> dimension1,
		IEnumerable<T2> dimension2,
		IEnumerable<T3> dimension3,
		IEnumerable<T4> dimension4,
		IEnumerable<T5> dimension5,
		IEnumerable<T6> dimension6,
		IEnumerable<T7> dimension7,
		IEnumerable<T8> dimension8,
		IEnumerable<T9> dimension9,
		IEnumerable<T10> dimension10)
	{
		Guard.ArgumentNotNull(dimension1);
		Guard.ArgumentNotNull(dimension2);
		Guard.ArgumentNotNull(dimension3);
		Guard.ArgumentNotNull(dimension4);
		Guard.ArgumentNotNull(dimension5);
		Guard.ArgumentNotNull(dimension6);
		Guard.ArgumentNotNull(dimension7);
		Guard.ArgumentNotNull(dimension8);
		Guard.ArgumentNotNull(dimension9);
		Guard.ArgumentNotNull(dimension10);

		var data1Empty = true;
		var data2Empty = true;
		var data3Empty = true;
		var data4Empty = true;
		var data5Empty = true;
		var data6Empty = true;
		var data7Empty = true;
		var data8Empty = true;
		var data9Empty = true;
		var data10Empty = true;

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

							foreach (var t6 in dimension6)
							{
								data6Empty = false;

								foreach (var t7 in dimension7)
								{
									data7Empty = false;

									foreach (var t8 in dimension8)
									{
										data8Empty = false;

										foreach (var t9 in dimension9)
										{
											data9Empty = false;

											foreach (var t10 in dimension10)
											{
												data10Empty = false;

												Add(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10);
											}
										}
									}
								}
							}
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
		Guard.ArgumentValid("Data dimension cannot be empty", !data6Empty, nameof(dimension6));
		Guard.ArgumentValid("Data dimension cannot be empty", !data7Empty, nameof(dimension7));
		Guard.ArgumentValid("Data dimension cannot be empty", !data8Empty, nameof(dimension8));
		Guard.ArgumentValid("Data dimension cannot be empty", !data9Empty, nameof(dimension9));
		Guard.ArgumentValid("Data dimension cannot be empty", !data10Empty, nameof(dimension10));
	}
}

/// <summary>
/// Represents theory data which is created from the merging of eleven data streams by
/// creating a matrix of the data.
/// </summary>
/// <typeparam name="T1">Type of the first data dimension</typeparam>
/// <typeparam name="T2">Type of the second data dimension</typeparam>
/// <typeparam name="T3">Type of the third data dimension</typeparam>
/// <typeparam name="T4">Type of the fourth data dimension</typeparam>
/// <typeparam name="T5">Type of the fifth data dimension</typeparam>
/// <typeparam name="T6">Type of the sixth data dimension</typeparam>
/// <typeparam name="T7">Type of the seventh data dimension</typeparam>
/// <typeparam name="T8">Type of the eighth data dimension</typeparam>
/// <typeparam name="T9">Type of the ninth data dimension</typeparam>
/// <typeparam name="T10">Type of the tenth data dimension</typeparam>
/// <typeparam name="T11">Type of the eleventh data dimension</typeparam>
public class MatrixTheoryData<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : TheoryData<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="MatrixTheoryData{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11}"/> class.
	/// </summary>
	/// <param name="dimension1">Data for the first dimension</param>
	/// <param name="dimension2">Data for the second dimension</param>
	/// <param name="dimension3">Data for the third dimension</param>
	/// <param name="dimension4">Data for the fourth dimension</param>
	/// <param name="dimension5">Data for the fifth dimension</param>
	/// <param name="dimension6">Data for the sixth dimension</param>
	/// <param name="dimension7">Data for the seventh dimension</param>
	/// <param name="dimension8">Data for the eighth dimension</param>
	/// <param name="dimension9">Data for the ninth dimension</param>
	/// <param name="dimension10">Data for the tenth dimension</param>
	/// <param name="dimension11">Data for the eleventh dimension</param>
	public MatrixTheoryData(
		IEnumerable<T1> dimension1,
		IEnumerable<T2> dimension2,
		IEnumerable<T3> dimension3,
		IEnumerable<T4> dimension4,
		IEnumerable<T5> dimension5,
		IEnumerable<T6> dimension6,
		IEnumerable<T7> dimension7,
		IEnumerable<T8> dimension8,
		IEnumerable<T9> dimension9,
		IEnumerable<T10> dimension10,
		IEnumerable<T11> dimension11)
	{
		Guard.ArgumentNotNull(dimension1);
		Guard.ArgumentNotNull(dimension2);
		Guard.ArgumentNotNull(dimension3);
		Guard.ArgumentNotNull(dimension4);
		Guard.ArgumentNotNull(dimension5);
		Guard.ArgumentNotNull(dimension6);
		Guard.ArgumentNotNull(dimension7);
		Guard.ArgumentNotNull(dimension8);
		Guard.ArgumentNotNull(dimension9);
		Guard.ArgumentNotNull(dimension10);
		Guard.ArgumentNotNull(dimension11);

		var data1Empty = true;
		var data2Empty = true;
		var data3Empty = true;
		var data4Empty = true;
		var data5Empty = true;
		var data6Empty = true;
		var data7Empty = true;
		var data8Empty = true;
		var data9Empty = true;
		var data10Empty = true;
		var data11Empty = true;

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

							foreach (var t6 in dimension6)
							{
								data6Empty = false;

								foreach (var t7 in dimension7)
								{
									data7Empty = false;

									foreach (var t8 in dimension8)
									{
										data8Empty = false;

										foreach (var t9 in dimension9)
										{
											data9Empty = false;

											foreach (var t10 in dimension10)
											{
												data10Empty = false;

												foreach (var t11 in dimension11)
												{
													data11Empty = false;

													Add(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11);
												}
											}
										}
									}
								}
							}
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
		Guard.ArgumentValid("Data dimension cannot be empty", !data6Empty, nameof(dimension6));
		Guard.ArgumentValid("Data dimension cannot be empty", !data7Empty, nameof(dimension7));
		Guard.ArgumentValid("Data dimension cannot be empty", !data8Empty, nameof(dimension8));
		Guard.ArgumentValid("Data dimension cannot be empty", !data9Empty, nameof(dimension9));
		Guard.ArgumentValid("Data dimension cannot be empty", !data10Empty, nameof(dimension10));
		Guard.ArgumentValid("Data dimension cannot be empty", !data11Empty, nameof(dimension11));
	}
}

/// <summary>
/// Represents theory data which is created from the merging of twelve data streams by
/// creating a matrix of the data.
/// </summary>
/// <typeparam name="T1">Type of the first data dimension</typeparam>
/// <typeparam name="T2">Type of the second data dimension</typeparam>
/// <typeparam name="T3">Type of the third data dimension</typeparam>
/// <typeparam name="T4">Type of the fourth data dimension</typeparam>
/// <typeparam name="T5">Type of the fifth data dimension</typeparam>
/// <typeparam name="T6">Type of the sixth data dimension</typeparam>
/// <typeparam name="T7">Type of the seventh data dimension</typeparam>
/// <typeparam name="T8">Type of the eighth data dimension</typeparam>
/// <typeparam name="T9">Type of the ninth data dimension</typeparam>
/// <typeparam name="T10">Type of the tenth data dimension</typeparam>
/// <typeparam name="T11">Type of the eleventh data dimension</typeparam>
/// <typeparam name="T12">Type of the twelfth data dimension</typeparam>
public class MatrixTheoryData<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : TheoryData<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="MatrixTheoryData{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12}"/> class.
	/// </summary>
	/// <param name="dimension1">Data for the first dimension</param>
	/// <param name="dimension2">Data for the second dimension</param>
	/// <param name="dimension3">Data for the third dimension</param>
	/// <param name="dimension4">Data for the fourth dimension</param>
	/// <param name="dimension5">Data for the fifth dimension</param>
	/// <param name="dimension6">Data for the sixth dimension</param>
	/// <param name="dimension7">Data for the seventh dimension</param>
	/// <param name="dimension8">Data for the eighth dimension</param>
	/// <param name="dimension9">Data for the ninth dimension</param>
	/// <param name="dimension10">Data for the tenth dimension</param>
	/// <param name="dimension11">Data for the eleventh dimension</param>
	/// <param name="dimension12">Data for the twelfth dimension</param>
	public MatrixTheoryData(
		IEnumerable<T1> dimension1,
		IEnumerable<T2> dimension2,
		IEnumerable<T3> dimension3,
		IEnumerable<T4> dimension4,
		IEnumerable<T5> dimension5,
		IEnumerable<T6> dimension6,
		IEnumerable<T7> dimension7,
		IEnumerable<T8> dimension8,
		IEnumerable<T9> dimension9,
		IEnumerable<T10> dimension10,
		IEnumerable<T11> dimension11,
		IEnumerable<T12> dimension12)
	{
		Guard.ArgumentNotNull(dimension1);
		Guard.ArgumentNotNull(dimension2);
		Guard.ArgumentNotNull(dimension3);
		Guard.ArgumentNotNull(dimension4);
		Guard.ArgumentNotNull(dimension5);
		Guard.ArgumentNotNull(dimension6);
		Guard.ArgumentNotNull(dimension7);
		Guard.ArgumentNotNull(dimension8);
		Guard.ArgumentNotNull(dimension9);
		Guard.ArgumentNotNull(dimension10);
		Guard.ArgumentNotNull(dimension11);
		Guard.ArgumentNotNull(dimension12);

		var data1Empty = true;
		var data2Empty = true;
		var data3Empty = true;
		var data4Empty = true;
		var data5Empty = true;
		var data6Empty = true;
		var data7Empty = true;
		var data8Empty = true;
		var data9Empty = true;
		var data10Empty = true;
		var data11Empty = true;
		var data12Empty = true;

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

							foreach (var t6 in dimension6)
							{
								data6Empty = false;

								foreach (var t7 in dimension7)
								{
									data7Empty = false;

									foreach (var t8 in dimension8)
									{
										data8Empty = false;

										foreach (var t9 in dimension9)
										{
											data9Empty = false;

											foreach (var t10 in dimension10)
											{
												data10Empty = false;

												foreach (var t11 in dimension11)
												{
													data11Empty = false;

													foreach (var t12 in dimension12)
													{
														data12Empty = false;

														Add(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12);
													}
												}
											}
										}
									}
								}
							}
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
		Guard.ArgumentValid("Data dimension cannot be empty", !data6Empty, nameof(dimension6));
		Guard.ArgumentValid("Data dimension cannot be empty", !data7Empty, nameof(dimension7));
		Guard.ArgumentValid("Data dimension cannot be empty", !data8Empty, nameof(dimension8));
		Guard.ArgumentValid("Data dimension cannot be empty", !data9Empty, nameof(dimension9));
		Guard.ArgumentValid("Data dimension cannot be empty", !data10Empty, nameof(dimension10));
		Guard.ArgumentValid("Data dimension cannot be empty", !data11Empty, nameof(dimension11));
		Guard.ArgumentValid("Data dimension cannot be empty", !data12Empty, nameof(dimension12));
	}
}

/// <summary>
/// Represents theory data which is created from the merging of thirteen data streams by
/// creating a matrix of the data.
/// </summary>
/// <typeparam name="T1">Type of the first data dimension</typeparam>
/// <typeparam name="T2">Type of the second data dimension</typeparam>
/// <typeparam name="T3">Type of the third data dimension</typeparam>
/// <typeparam name="T4">Type of the fourth data dimension</typeparam>
/// <typeparam name="T5">Type of the fifth data dimension</typeparam>
/// <typeparam name="T6">Type of the sixth data dimension</typeparam>
/// <typeparam name="T7">Type of the seventh data dimension</typeparam>
/// <typeparam name="T8">Type of the eighth data dimension</typeparam>
/// <typeparam name="T9">Type of the ninth data dimension</typeparam>
/// <typeparam name="T10">Type of the tenth data dimension</typeparam>
/// <typeparam name="T11">Type of the eleventh data dimension</typeparam>
/// <typeparam name="T12">Type of the twelfth data dimension</typeparam>
/// <typeparam name="T13">Type of the thirteenth data dimension</typeparam>
public class MatrixTheoryData<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> : TheoryData<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="MatrixTheoryData{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13}"/> class.
	/// </summary>
	/// <param name="dimension1">Data for the first dimension</param>
	/// <param name="dimension2">Data for the second dimension</param>
	/// <param name="dimension3">Data for the third dimension</param>
	/// <param name="dimension4">Data for the fourth dimension</param>
	/// <param name="dimension5">Data for the fifth dimension</param>
	/// <param name="dimension6">Data for the sixth dimension</param>
	/// <param name="dimension7">Data for the seventh dimension</param>
	/// <param name="dimension8">Data for the eighth dimension</param>
	/// <param name="dimension9">Data for the ninth dimension</param>
	/// <param name="dimension10">Data for the tenth dimension</param>
	/// <param name="dimension11">Data for the eleventh dimension</param>
	/// <param name="dimension12">Data for the twelfth dimension</param>
	/// <param name="dimension13">Data for the thirteenth dimension</param>
	public MatrixTheoryData(
		IEnumerable<T1> dimension1,
		IEnumerable<T2> dimension2,
		IEnumerable<T3> dimension3,
		IEnumerable<T4> dimension4,
		IEnumerable<T5> dimension5,
		IEnumerable<T6> dimension6,
		IEnumerable<T7> dimension7,
		IEnumerable<T8> dimension8,
		IEnumerable<T9> dimension9,
		IEnumerable<T10> dimension10,
		IEnumerable<T11> dimension11,
		IEnumerable<T12> dimension12,
		IEnumerable<T13> dimension13)
	{
		Guard.ArgumentNotNull(dimension1);
		Guard.ArgumentNotNull(dimension2);
		Guard.ArgumentNotNull(dimension3);
		Guard.ArgumentNotNull(dimension4);
		Guard.ArgumentNotNull(dimension5);
		Guard.ArgumentNotNull(dimension6);
		Guard.ArgumentNotNull(dimension7);
		Guard.ArgumentNotNull(dimension8);
		Guard.ArgumentNotNull(dimension9);
		Guard.ArgumentNotNull(dimension10);
		Guard.ArgumentNotNull(dimension11);
		Guard.ArgumentNotNull(dimension12);
		Guard.ArgumentNotNull(dimension13);

		var data1Empty = true;
		var data2Empty = true;
		var data3Empty = true;
		var data4Empty = true;
		var data5Empty = true;
		var data6Empty = true;
		var data7Empty = true;
		var data8Empty = true;
		var data9Empty = true;
		var data10Empty = true;
		var data11Empty = true;
		var data12Empty = true;
		var data13Empty = true;

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

							foreach (var t6 in dimension6)
							{
								data6Empty = false;

								foreach (var t7 in dimension7)
								{
									data7Empty = false;

									foreach (var t8 in dimension8)
									{
										data8Empty = false;

										foreach (var t9 in dimension9)
										{
											data9Empty = false;

											foreach (var t10 in dimension10)
											{
												data10Empty = false;

												foreach (var t11 in dimension11)
												{
													data11Empty = false;

													foreach (var t12 in dimension12)
													{
														data12Empty = false;

														foreach (var t13 in dimension13)
														{
															data13Empty = false;

															Add(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13);
														}
													}
												}
											}
										}
									}
								}
							}
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
		Guard.ArgumentValid("Data dimension cannot be empty", !data6Empty, nameof(dimension6));
		Guard.ArgumentValid("Data dimension cannot be empty", !data7Empty, nameof(dimension7));
		Guard.ArgumentValid("Data dimension cannot be empty", !data8Empty, nameof(dimension8));
		Guard.ArgumentValid("Data dimension cannot be empty", !data9Empty, nameof(dimension9));
		Guard.ArgumentValid("Data dimension cannot be empty", !data10Empty, nameof(dimension10));
		Guard.ArgumentValid("Data dimension cannot be empty", !data11Empty, nameof(dimension11));
		Guard.ArgumentValid("Data dimension cannot be empty", !data12Empty, nameof(dimension12));
		Guard.ArgumentValid("Data dimension cannot be empty", !data13Empty, nameof(dimension13));
	}
}

/// <summary>
/// Represents theory data which is created from the merging of fourteen data streams by
/// creating a matrix of the data.
/// </summary>
/// <typeparam name="T1">Type of the first data dimension</typeparam>
/// <typeparam name="T2">Type of the second data dimension</typeparam>
/// <typeparam name="T3">Type of the third data dimension</typeparam>
/// <typeparam name="T4">Type of the fourth data dimension</typeparam>
/// <typeparam name="T5">Type of the fifth data dimension</typeparam>
/// <typeparam name="T6">Type of the sixth data dimension</typeparam>
/// <typeparam name="T7">Type of the seventh data dimension</typeparam>
/// <typeparam name="T8">Type of the eighth data dimension</typeparam>
/// <typeparam name="T9">Type of the ninth data dimension</typeparam>
/// <typeparam name="T10">Type of the tenth data dimension</typeparam>
/// <typeparam name="T11">Type of the eleventh data dimension</typeparam>
/// <typeparam name="T12">Type of the twelfth data dimension</typeparam>
/// <typeparam name="T13">Type of the thirteenth data dimension</typeparam>
/// <typeparam name="T14">Type of the fourteenth data dimension</typeparam>
public class MatrixTheoryData<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> : TheoryData<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="MatrixTheoryData{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14}"/> class.
	/// </summary>
	/// <param name="dimension1">Data for the first dimension</param>
	/// <param name="dimension2">Data for the second dimension</param>
	/// <param name="dimension3">Data for the third dimension</param>
	/// <param name="dimension4">Data for the fourth dimension</param>
	/// <param name="dimension5">Data for the fifth dimension</param>
	/// <param name="dimension6">Data for the sixth dimension</param>
	/// <param name="dimension7">Data for the seventh dimension</param>
	/// <param name="dimension8">Data for the eighth dimension</param>
	/// <param name="dimension9">Data for the ninth dimension</param>
	/// <param name="dimension10">Data for the tenth dimension</param>
	/// <param name="dimension11">Data for the eleventh dimension</param>
	/// <param name="dimension12">Data for the twelfth dimension</param>
	/// <param name="dimension13">Data for the thirteenth dimension</param>
	/// <param name="dimension14">Data for the fourteenth dimension</param>
	public MatrixTheoryData(
		IEnumerable<T1> dimension1,
		IEnumerable<T2> dimension2,
		IEnumerable<T3> dimension3,
		IEnumerable<T4> dimension4,
		IEnumerable<T5> dimension5,
		IEnumerable<T6> dimension6,
		IEnumerable<T7> dimension7,
		IEnumerable<T8> dimension8,
		IEnumerable<T9> dimension9,
		IEnumerable<T10> dimension10,
		IEnumerable<T11> dimension11,
		IEnumerable<T12> dimension12,
		IEnumerable<T13> dimension13,
		IEnumerable<T14> dimension14)
	{
		Guard.ArgumentNotNull(dimension1);
		Guard.ArgumentNotNull(dimension2);
		Guard.ArgumentNotNull(dimension3);
		Guard.ArgumentNotNull(dimension4);
		Guard.ArgumentNotNull(dimension5);
		Guard.ArgumentNotNull(dimension6);
		Guard.ArgumentNotNull(dimension7);
		Guard.ArgumentNotNull(dimension8);
		Guard.ArgumentNotNull(dimension9);
		Guard.ArgumentNotNull(dimension10);
		Guard.ArgumentNotNull(dimension11);
		Guard.ArgumentNotNull(dimension12);
		Guard.ArgumentNotNull(dimension13);
		Guard.ArgumentNotNull(dimension14);

		var data1Empty = true;
		var data2Empty = true;
		var data3Empty = true;
		var data4Empty = true;
		var data5Empty = true;
		var data6Empty = true;
		var data7Empty = true;
		var data8Empty = true;
		var data9Empty = true;
		var data10Empty = true;
		var data11Empty = true;
		var data12Empty = true;
		var data13Empty = true;
		var data14Empty = true;

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

							foreach (var t6 in dimension6)
							{
								data6Empty = false;

								foreach (var t7 in dimension7)
								{
									data7Empty = false;

									foreach (var t8 in dimension8)
									{
										data8Empty = false;

										foreach (var t9 in dimension9)
										{
											data9Empty = false;

											foreach (var t10 in dimension10)
											{
												data10Empty = false;

												foreach (var t11 in dimension11)
												{
													data11Empty = false;

													foreach (var t12 in dimension12)
													{
														data12Empty = false;

														foreach (var t13 in dimension13)
														{
															data13Empty = false;

															foreach (var t14 in dimension14)
															{
																data14Empty = false;

																Add(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14);
															}
														}
													}
												}
											}
										}
									}
								}
							}
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
		Guard.ArgumentValid("Data dimension cannot be empty", !data6Empty, nameof(dimension6));
		Guard.ArgumentValid("Data dimension cannot be empty", !data7Empty, nameof(dimension7));
		Guard.ArgumentValid("Data dimension cannot be empty", !data8Empty, nameof(dimension8));
		Guard.ArgumentValid("Data dimension cannot be empty", !data9Empty, nameof(dimension9));
		Guard.ArgumentValid("Data dimension cannot be empty", !data10Empty, nameof(dimension10));
		Guard.ArgumentValid("Data dimension cannot be empty", !data11Empty, nameof(dimension11));
		Guard.ArgumentValid("Data dimension cannot be empty", !data12Empty, nameof(dimension12));
		Guard.ArgumentValid("Data dimension cannot be empty", !data13Empty, nameof(dimension13));
		Guard.ArgumentValid("Data dimension cannot be empty", !data14Empty, nameof(dimension14));
	}
}

/// <summary>
/// Represents theory data which is created from the merging of fifteen data streams by
/// creating a matrix of the data.
/// </summary>
/// <typeparam name="T1">Type of the first data dimension</typeparam>
/// <typeparam name="T2">Type of the second data dimension</typeparam>
/// <typeparam name="T3">Type of the third data dimension</typeparam>
/// <typeparam name="T4">Type of the fourth data dimension</typeparam>
/// <typeparam name="T5">Type of the fifth data dimension</typeparam>
/// <typeparam name="T6">Type of the sixth data dimension</typeparam>
/// <typeparam name="T7">Type of the seventh data dimension</typeparam>
/// <typeparam name="T8">Type of the eighth data dimension</typeparam>
/// <typeparam name="T9">Type of the ninth data dimension</typeparam>
/// <typeparam name="T10">Type of the tenth data dimension</typeparam>
/// <typeparam name="T11">Type of the eleventh data dimension</typeparam>
/// <typeparam name="T12">Type of the twelfth data dimension</typeparam>
/// <typeparam name="T13">Type of the thirteenth data dimension</typeparam>
/// <typeparam name="T14">Type of the fourteenth data dimension</typeparam>
/// <typeparam name="T15">Type of the fifteenth data dimension</typeparam>
public class MatrixTheoryData<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> : TheoryData<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="MatrixTheoryData{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15}"/> class.
	/// </summary>
	/// <param name="dimension1">Data for the first dimension</param>
	/// <param name="dimension2">Data for the second dimension</param>
	/// <param name="dimension3">Data for the third dimension</param>
	/// <param name="dimension4">Data for the fourth dimension</param>
	/// <param name="dimension5">Data for the fifth dimension</param>
	/// <param name="dimension6">Data for the sixth dimension</param>
	/// <param name="dimension7">Data for the seventh dimension</param>
	/// <param name="dimension8">Data for the eighth dimension</param>
	/// <param name="dimension9">Data for the ninth dimension</param>
	/// <param name="dimension10">Data for the tenth dimension</param>
	/// <param name="dimension11">Data for the eleventh dimension</param>
	/// <param name="dimension12">Data for the twelfth dimension</param>
	/// <param name="dimension13">Data for the thirteenth dimension</param>
	/// <param name="dimension14">Data for the fourteenth dimension</param>
	/// <param name="dimension15">Data for the fifteenth dimension</param>
	public MatrixTheoryData(
		IEnumerable<T1> dimension1,
		IEnumerable<T2> dimension2,
		IEnumerable<T3> dimension3,
		IEnumerable<T4> dimension4,
		IEnumerable<T5> dimension5,
		IEnumerable<T6> dimension6,
		IEnumerable<T7> dimension7,
		IEnumerable<T8> dimension8,
		IEnumerable<T9> dimension9,
		IEnumerable<T10> dimension10,
		IEnumerable<T11> dimension11,
		IEnumerable<T12> dimension12,
		IEnumerable<T13> dimension13,
		IEnumerable<T14> dimension14,
		IEnumerable<T15> dimension15)
	{
		Guard.ArgumentNotNull(dimension1);
		Guard.ArgumentNotNull(dimension2);
		Guard.ArgumentNotNull(dimension3);
		Guard.ArgumentNotNull(dimension4);
		Guard.ArgumentNotNull(dimension5);
		Guard.ArgumentNotNull(dimension6);
		Guard.ArgumentNotNull(dimension7);
		Guard.ArgumentNotNull(dimension8);
		Guard.ArgumentNotNull(dimension9);
		Guard.ArgumentNotNull(dimension10);
		Guard.ArgumentNotNull(dimension11);
		Guard.ArgumentNotNull(dimension12);
		Guard.ArgumentNotNull(dimension13);
		Guard.ArgumentNotNull(dimension14);
		Guard.ArgumentNotNull(dimension15);

		var data1Empty = true;
		var data2Empty = true;
		var data3Empty = true;
		var data4Empty = true;
		var data5Empty = true;
		var data6Empty = true;
		var data7Empty = true;
		var data8Empty = true;
		var data9Empty = true;
		var data10Empty = true;
		var data11Empty = true;
		var data12Empty = true;
		var data13Empty = true;
		var data14Empty = true;
		var data15Empty = true;

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

							foreach (var t6 in dimension6)
							{
								data6Empty = false;

								foreach (var t7 in dimension7)
								{
									data7Empty = false;

									foreach (var t8 in dimension8)
									{
										data8Empty = false;

										foreach (var t9 in dimension9)
										{
											data9Empty = false;

											foreach (var t10 in dimension10)
											{
												data10Empty = false;

												foreach (var t11 in dimension11)
												{
													data11Empty = false;

													foreach (var t12 in dimension12)
													{
														data12Empty = false;

														foreach (var t13 in dimension13)
														{
															data13Empty = false;

															foreach (var t14 in dimension14)
															{
																data14Empty = false;

																foreach (var t15 in dimension15)
																{
																	data15Empty = false;

																	Add(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14, t15);
																}
															}
														}
													}
												}
											}
										}
									}
								}
							}
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
		Guard.ArgumentValid("Data dimension cannot be empty", !data6Empty, nameof(dimension6));
		Guard.ArgumentValid("Data dimension cannot be empty", !data7Empty, nameof(dimension7));
		Guard.ArgumentValid("Data dimension cannot be empty", !data8Empty, nameof(dimension8));
		Guard.ArgumentValid("Data dimension cannot be empty", !data9Empty, nameof(dimension9));
		Guard.ArgumentValid("Data dimension cannot be empty", !data10Empty, nameof(dimension10));
		Guard.ArgumentValid("Data dimension cannot be empty", !data11Empty, nameof(dimension11));
		Guard.ArgumentValid("Data dimension cannot be empty", !data12Empty, nameof(dimension12));
		Guard.ArgumentValid("Data dimension cannot be empty", !data13Empty, nameof(dimension13));
		Guard.ArgumentValid("Data dimension cannot be empty", !data14Empty, nameof(dimension14));
		Guard.ArgumentValid("Data dimension cannot be empty", !data15Empty, nameof(dimension15));
	}
}
