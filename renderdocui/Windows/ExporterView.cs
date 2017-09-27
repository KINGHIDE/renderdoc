using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using renderdocui.Code;
using renderdoc;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections;

namespace renderdocui.Windows
{
    public struct Quat
    {
        public double x;
        public double y;
        public double z;
        public double w;

        public Quat(double _x, double _y, double _z, double _w)
        {
            x = _x;
            y = _y;
            z = _z;
            w = _w;
        }

        public void GetEulerAngles(out double yaw, out double pitch, out double roll)
        {
            pitch = Math.Atan2(2 * (w * x + y * z), 1 - 2 * (x * x + y * y));
            yaw = Math.Asin(2 * (w * y - z * x));
            roll = Math.Atan2(2 * (w * z + x * y), 1 - 2 * (y * y + z * z));
        }


        public static Quat Identity()
        {
            Quat r = new Quat(0.0, 0.0, 0.0, 1.0);
            return r;
        }

        public static Quat FromEuler(double yaw, double pitch, double roll)
        {
            Quat r = new Quat();

            pitch *= 0.5;
            yaw *= 0.5;
            roll *= 0.5;

            double sinp = Math.Sin(pitch);
            double siny = Math.Sin(yaw);
            double sinr = Math.Sin(roll);
            double cosp = Math.Cos(pitch);
            double cosy = Math.Cos(yaw);
            double cosr = Math.Cos(roll);

            r.w = cosp * cosy * cosr + sinp * siny * sinr;
            r.x = sinp * cosy * cosr - cosp * siny * sinr;
            r.y = cosp * siny * cosr + sinp * cosy * sinr;
            r.z = cosp * cosy * sinr - sinp * siny * cosr;

            return r;
        }

        public static Quat FromRotationMatrix(Vec3 axisX, Vec3 axisY, Vec3 axisZ)
        {
            Quat r = new Quat();
            double trace = axisX.x + axisY.y + axisZ.z + 1.0;

            if (trace > 1.0)
            {
                double s = 0.5 / Math.Sqrt(trace);
                r.w = 0.25 / s;
                r.x = (axisY.z - axisZ.y) * s;
                r.y = (axisZ.x - axisX.z) * s;
                r.z = (axisX.y - axisY.x) * s;
            }
            else
            {
                // Note: since xaxis, yaxis, and zaxis are normalized, 
                // we will never divide by zero in the code below.
                if (axisX.x > axisY.y && axisX.x > axisZ.z)
                {
                    double s = 0.5 / Math.Sqrt(1.0 + axisX.x - axisY.y - axisZ.z);
                    r.w = (axisY.z - axisZ.y) * s;
                    r.x = 0.25 / s;
                    r.y = (axisY.x + axisX.y) * s;
                    r.z = (axisZ.x + axisX.z) * s;
                }
                else if (axisY.y > axisZ.z)
                {
                    double s = 0.5 / Math.Sqrt(1.0 + axisY.y - axisX.x - axisZ.z);
                    r.w = (axisZ.x - axisX.z) * s;
                    r.x = (axisY.x + axisX.y) * s;
                    r.y = 0.25 / s;
                    r.z = (axisZ.y + axisY.z) * s;
                }
                else
                {
                    double s = 0.5 / Math.Sqrt(1.0 + axisZ.z - axisX.x - axisY.y);
                    r.w = (axisX.y - axisY.x) * s;
                    r.x = (axisZ.x + axisX.z) * s;
                    r.y = (axisZ.y + axisY.z) * s;
                    r.z = 0.25 / s;
                }
            }

            return r;
        }
    }

    public struct Mat4x4
    {
        public double _00; // 0
        public double _10; // 1
        public double _20; // 2
        public double _30; // 3
        public double _01; // 4
        public double _11; // 5
        public double _21; // 6
        public double _31; // 7
        public double _02; // 8
        public double _12; // 9
        public double _22; // 10
        public double _32; // 11
        public double _03; // 12
        public double _13; // 13
        public double _23; // 14
        public double _33; // 15


        public Mat4x4(
            double __00, double __01, double __02, double __03,
            double __10, double __11, double __12, double __13,
            double __20, double __21, double __22, double __23,
            double __30, double __31, double __32, double __33)
        {
            _00 = __00;
            _10 = __10;
            _20 = __20;
            _30 = __30;

            _01 = __01;
            _11 = __11;
            _21 = __21;
            _31 = __31;

            _02 = __02;
            _12 = __12;
            _22 = __22;
            _32 = __32;

            _03 = __03;
            _13 = __13;
            _23 = __23;
            _33 = __33;
        }

        public static Vec4 operator *(Mat4x4 m, Vec4 v)
        {
            return new Vec4(
                m._00 * v.x + m._01 * v.y + m._02 * v.z + m._03 * v.w,
                m._10 * v.x + m._11 * v.y + m._12 * v.z + m._13 * v.w,
                m._20 * v.x + m._21 * v.y + m._22 * v.z + m._23 * v.w,
                m._30 * v.x + m._31 * v.y + m._32 * v.z + m._33 * v.w);
        }

        public static Vec4 operator *(Vec4 v, Mat4x4 m)
        {
            return new Vec4(
                m._00 * v.x + m._10 * v.y + m._20 * v.z + m._30 * v.w,
                m._01 * v.x + m._11 * v.y + m._21 * v.z + m._31 * v.w,
                m._02 * v.x + m._12 * v.y + m._22 * v.z + m._32 * v.w,
                m._03 * v.x + m._13 * v.y + m._23 * v.z + m._33 * v.w);
        }


        public static Vec3 MulRotation(Mat4x4 m, Vec3 v)
        {
            return new Vec3(
                m._00 * v.x + m._01 * v.y + m._02 * v.z,
                m._10 * v.x + m._11 * v.y + m._12 * v.z,
                m._20 * v.x + m._21 * v.y + m._22 * v.z);
        }

        public static Vec3 MulRotation(Vec3 v, Mat4x4 m)
        {
            return new Vec3(
                m._00 * v.x + m._10 * v.y + m._20 * v.z,
                m._01 * v.x + m._11 * v.y + m._21 * v.z,
                m._02 * v.x + m._12 * v.y + m._22 * v.z);
        }


        public static Mat4x4 operator *(Mat4x4 a, Mat4x4 b)
        {
            Mat4x4 r = new Mat4x4();

            Vec4 x = a * b.GetX();
            Vec4 y = a * b.GetY();
            Vec4 z = a * b.GetZ();
            Vec4 w = a * b.GetW();

            r._00 = x.x;
            r._10 = x.y;
            r._20 = x.z;
            r._30 = x.w;

            r._01 = y.x;
            r._11 = y.y;
            r._21 = y.z;
            r._31 = y.w;

            r._02 = z.x;
            r._12 = z.y;
            r._22 = z.z;
            r._32 = z.w;

            r._03 = w.x;
            r._13 = w.y;
            r._23 = w.z;
            r._33 = w.w;

            return r;
        }

        private Vec4 GetX()
        {
            return new Vec4(_00, _10, _20, _30);
        }

        private Vec4 GetY()
        {
            return new Vec4(_01, _11, _21, _31);
        }

        private Vec4 GetZ()
        {
            return new Vec4(_02, _12, _22, _32);
        }

        private Vec4 GetW()
        {
            return new Vec4(_03, _13, _23, _33);
        }

        public static Mat4x4 Identity()
        {
            return new Mat4x4(
                1.0f, 0.0f, 0.0f, 0.0f,
                0.0f, 1.0f, 0.0f, 0.0f,
                0.0f, 0.0f, 1.0f, 0.0f,
                0.0f, 0.0f, 0.0f, 1.0f);
        }

        public int NumberOfZeroesInsideRotationComponent()
        {
            double m00 = Math.Abs(_00);
            double m01 = Math.Abs(_01);
            double m02 = Math.Abs(_02);
            double m10 = Math.Abs(_10);
            double m11 = Math.Abs(_11);
            double m12 = Math.Abs(_12);
            double m20 = Math.Abs(_20);
            double m21 = Math.Abs(_21);
            double m22 = Math.Abs(_22);

            double eps = 0.001;
            int zeroCount = 0;

            if (m00 < eps)
                zeroCount++;
            if (m01 < eps)
                zeroCount++;
            if (m02 < eps)
                zeroCount++;
            if (m10 < eps)
                zeroCount++;
            if (m11 < eps)
                zeroCount++;
            if (m12 < eps)
                zeroCount++;
            if (m20 < eps)
                zeroCount++;
            if (m21 < eps)
                zeroCount++;
            if (m22 < eps)
                zeroCount++;

            return zeroCount;
        }



        public bool Inverse()
        {
            Mat4x4 m = this;

            double m2233 = m._22 * m._33 - m._32 * m._23;
            double m1233 = m._12 * m._33 - m._32 * m._13;
            double m1223 = m._12 * m._23 - m._22 * m._13;
            double m2133 = m._21 * m._33 - m._31 * m._23;
            double m1133 = m._11 * m._33 - m._31 * m._13;
            double m1123 = m._11 * m._23 - m._21 * m._13;
            double m2132 = m._21 * m._32 - m._31 * m._22;
            double m1132 = m._11 * m._32 - m._31 * m._12;
            double m1122 = m._11 * m._22 - m._21 * m._12;
            double m0233 = m._02 * m._33 - m._32 * m._03;
            double m0223 = m._02 * m._23 - m._22 * m._03;
            double m0133 = m._01 * m._33 - m._31 * m._03;
            double m0123 = m._01 * m._23 - m._21 * m._03;
            double m0132 = m._01 * m._32 - m._31 * m._02;
            double m0122 = m._01 * m._22 - m._21 * m._02;
            double m0213 = m._02 * m._13 - m._12 * m._03;
            double m0113 = m._01 * m._13 - m._11 * m._03;
            double m0112 = m._01 * m._12 - m._11 * m._02;

            // Adjoint Matrix
            _00 = m._11 * m2233 - m._21 * m1233 + m._31 * m1223;
            _10 = -m._10 * m2233 + m._20 * m1233 - m._30 * m1223;
            _20 = m._10 * m2133 - m._20 * m1133 + m._30 * m1123;
            _30 = -m._10 * m2132 + m._20 * m1132 - m._30 * m1122;

            _01 = -m._01 * m2233 + m._21 * m0233 - m._31 * m0223;
            _11 = m._00 * m2233 - m._20 * m0233 + m._30 * m0223;
            _21 = -m._00 * m2133 + m._20 * m0133 - m._30 * m0123;
            _31 = m._00 * m2132 - m._20 * m0132 + m._30 * m0122;

            _02 = m._01 * m1233 - m._11 * m0233 + m._31 * m0213;
            _12 = -m._00 * m1233 + m._10 * m0233 - m._30 * m0213;
            _22 = m._00 * m1133 - m._10 * m0133 + m._30 * m0113;
            _32 = -m._00 * m1132 + m._10 * m0132 - m._30 * m0112;

            _03 = -m._01 * m1223 + m._11 * m0223 - m._21 * m0213;
            _13 = m._00 * m1223 - m._10 * m0223 + m._20 * m0213;
            _23 = -m._00 * m1123 + m._10 * m0123 - m._20 * m0113;
            _33 = m._00 * m1122 - m._10 * m0122 + m._20 * m0112;

            // Division by determinant
            double fDet = m._00 * _00 + m._10 * _01 + m._20 * _02 + m._30 * _03;

            if (Math.Abs(fDet) < 1e-16)
            {
                // Singular matrix found
                _00 = 1.0; _10 = 0.0; _20 = 0.0; _30 = 0.0;
                _01 = 0.0; _11 = 1.0; _21 = 0.0; _31 = 0.0;
                _02 = 0.0; _12 = 0.0; _22 = 1.0; _32 = 0.0;
                _03 = 0.0; _13 = 0.0; _23 = 0.0; _33 = 1.0;
                return false;
            }
            fDet = 1.0 / fDet;
            _00 *= fDet; _10 *= fDet; _20 *= fDet; _30 *= fDet;
            _01 *= fDet; _11 *= fDet; _21 *= fDet; _31 *= fDet;
            _02 *= fDet; _12 *= fDet; _22 *= fDet; _32 *= fDet;
            _03 *= fDet; _13 *= fDet; _23 *= fDet; _33 *= fDet;

            return true;
        }


        double Determinant()
        {
            double a0 = _00 * _11 - _10 * _01;
            double a1 = _00 * _21 - _20 * _01;
            double a2 = _00 * _31 - _30 * _01;
            double a3 = _10 * _21 - _20 * _11;
            double a4 = _10 * _31 - _30 * _11;
            double a5 = _20 * _31 - _30 * _21;

            double b0 = _02 * _13 - _12 * _03;
            double b1 = _02 * _23 - _22 * _03;
            double b2 = _02 * _33 - _32 * _03;
            double b3 = _12 * _23 - _22 * _13;
            double b4 = _12 * _33 - _32 * _13;
            double b5 = _22 * _33 - _32 * _23;

            // Calculate the determinant.
            return (a0* b5 - a1* b4 + a2* b3 + a3* b2 - a4* b1 + a5* b0);
        }


    public bool Decompose(out Vec3 translate, out Vec3 euler, out Vec3 scale)
        {
            translate = new Vec3(0.0, 0.0, 0.0);
            euler = new Vec3(0.0, 0.0, 0.0);
            scale = new Vec3(1.0, 1.0, 1.0);

            double eps = 0.01;
            if (_30 > eps || _31 > eps || _32 > eps || Math.Abs(_33 - 1.0) > eps)
            {
                //projective component in world matrix?
                return false;
            }

            translate = (this * new Vec4(0.0, 0.0, 0.0, 1.0)).HomogeneousToCartesian();
            //translate = new Vec3(_03, _13, _23) / _33;

            Vec3 axisX = new Vec3(_00, _10, _20);
            Vec3 axisY = new Vec3(_01, _11, _21);
            Vec3 axisZ = new Vec3(_02, _12, _22);

            scale = new Vec3(axisX.Length(), axisY.Length(), axisZ.Length());

            if (scale.x < 0.00001 || scale.y < 0.00001 || scale.z < 0.00001)
            {
                // zero scale
                return false;
            }

            // Determine if we have a negative scale (true if determinant is less than zero).
            // In this case, we simply negate a single axis of the scale.
            double d = Determinant();
            if (d < 0.0)
            {
                scale.z = -scale.z;
            }

            //normalize axes (get rotation matrix)
            axisX /= scale.x;
            axisY /= scale.y;
            axisZ /= scale.z;

/*
            // build OrthoNormalized rotation matrix
            Vec3 orthoNormalX = axisX;
            orthoNormalX.Normalize();

            Vec3 orthoNormalY = axisY;

            Vec3 orthoNormalZ = Vec3.Cross(orthoNormalX, orthoNormalY);
            orthoNormalZ.Normalize();

            orthoNormalY = Vec3.Cross(orthoNormalZ, orthoNormalX);
            //

            double s0 = Vec3.Dot(axisX, orthoNormalX);
            double s1 = Vec3.Dot(axisY, orthoNormalY);
            double s2 = Vec3.Dot(axisZ, orthoNormalZ);
*/
            Quat q = Quat.FromRotationMatrix(axisX, axisY, axisZ);

            double yaw = 0.0;
            double pitch = 0.0;
            double roll = 0.0;
            q.GetEulerAngles(out yaw, out pitch, out roll);

            //Quat q1 = Quat.FromEuler(yaw, pitch, roll);


            double rad2deg = 180.0 / Math.PI;

            // to degrees
            yaw *= rad2deg;
            pitch *= rad2deg;
            roll *= rad2deg;

            //euler = new Vec3(roll, yaw, pitch);
            euler = new Vec3(pitch, yaw, roll);

            return true;
        }
    }



    public struct Vec4
    {
        public double x;
        public double y;
        public double z;
        public double w;

        public Vec4(double _x, double _y, double _z, double _w)
        {
            x = _x;
            y = _y;
            z = _z;
            w = _w;
        }
        public Vec3 AsVector3()
        {
            return new Vec3(x, y, z);
        }

        public Vec3 HomogeneousToCartesian()
        {
            double denom = w;
            if (Math.Abs(denom) < 0.0000001)
            {
                return new Vec3(0.0, 0.0, 0.0);
            }
            return new Vec3(x / denom, y / denom, z / denom);
        }

       

    }

    public struct Vec2
    {
        public double x;
        public double y;

        public Vec2(double _x, double _y)
        {
            x = _x;
            y = _y;
        }

    }


    public struct Vec3
    {
        public double x;
        public double y;
        public double z;

        public Vec3(double _x, double _y, double _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }


        public Vec4 AsVector4()
        {
            return new Vec4(x, y, z, 1.0);
        }

        public static bool IsNearEqual(Vec3 a, Vec3 b, Vec3 eps)
        {
            if (Math.Abs(a.x - b.x) > eps.x)
            {
                return false;
            }

            if (Math.Abs(a.y - b.y) > eps.y)
            {
                return false;
            }

            if (Math.Abs(a.z - b.z) > eps.z)
            {
                return false;
            }

            return true;
        }


        public bool Normalize()
        {
            double l2 = SqLength();
            if (l2 > 0.00001)
            {
                double l = Math.Sqrt(l2);
                x /= l;
                y /= l;
                z /= l;
                return true;
            }

            return false;
        }

        public double SqLength()
        {
            double l2 = x * x + y * y + z * z;
            return l2;
        }

        public double Length()
        {
            double l = Math.Sqrt(SqLength());
            return l;
        }


        public static Vec3 Cross(Vec3 a, Vec3 b)
        {
            return new Vec3(
                a.y * b.z - b.y * a.z,
                a.z * b.x - b.z * a.x,
                a.x * b.y - b.x * a.y);
        }

        public static Vec3 Min(Vec3 a, Vec3 b)
        {
            return new Vec3(
                Math.Min(a.x, b.x),
                Math.Min(a.y, b.y),
                Math.Min(a.z, b.z));
        }

        public static Vec3 Max(Vec3 a, Vec3 b)
        {
            return new Vec3(
                Math.Max(a.x, b.x),
                Math.Max(a.y, b.y),
                Math.Max(a.z, b.z));
        }


        public static double Dot(Vec3 a, Vec3 b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }

        public static Vec3 operator -(Vec3 a)
        {
            return new Vec3(-a.x, -a.y, -a.z);
        }

        public static Vec3 operator -(Vec3 a, Vec3 b)
        {
            return new Vec3(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static Vec3 operator +(Vec3 a, Vec3 b)
        {
            return new Vec3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Vec3 operator *(Vec3 a, double b)
        {
            return new Vec3(a.x * b, a.y * b, a.z * b);
        }

        public static Vec3 operator /(Vec3 a, double b)
        {
            return new Vec3(a.x / b, a.y / b, a.z / b);
        }



    }


    public partial class ExporterView : DockContent, ILogViewerForm
    {
        private class VertexFbx
        {
            public Vec3 p = new Vec3(0.0, 0.0, 0.0);
            public Vec3 n = new Vec3(0.0, 0.0, 1.0);
            public Vec2 uv = new Vec2(0.0, 0.0);
        }


        private class VertexEx
        {
            //zero element
            public Vec3 e0 = new Vec3(0.0, 0.0, 0.0);

            //position
            public Vec3 p = new Vec3(0.0, 0.0, 0.0);
            public bool hasPosition = false;

            //post projective
            public Vec4 pp = new Vec4(0.0, 0.0, 0.0, 1.0);

            //normal
            public Vec3 n = new Vec3(0.0, 0.0, 0.0);
            public bool hasNormal = false;

            //tangent
            public Vec4 t = new Vec4(0.0, 0.0, 0.0, 0.0);

            //binormal
            public Vec4 b = new Vec4(0.0, 0.0, 0.0, 0.0);

            //color
            public Vec4 clr = new Vec4(1.0, 1.0, 1.0, 1.0);

            //uv0
            public Vec2 uv0 = new Vec2(0.0, 0.0);
            public bool hasUV = false;

            //uv1
            public Vec2 uv1 = new Vec2(0.0, 0.0);

            //uv2
            public Vec2 uv2 = new Vec2(0.0, 0.0);

            //uv3
            public Vec2 uv3 = new Vec2(0.0, 0.0);

            //blend weight
            public Vec4 blendWeight = new Vec4(0.0, 0.0, 0.0, 0.0);

            //blend indices
            public Vec4 blendIndices = new Vec4(0.0, 0.0, 0.0, 0.0);


            public void Finish()
            {
                if (hasPosition == false)
                {
                    p = e0;
                }

                if (hasNormal == false)
                {
                    Vec3 vt = t.AsVector3();
                    Vec3 vb = b.AsVector3();
                    if (!n.Normalize() && vt.Normalize() && vb.Normalize())
                    {
                        //try to reconstruct normal from binormal/tangent
                        n = Vec3.Cross(vt, vb);
                        if (t.w < 0.0f)
                        {
                            n = -n;
                        }
                        n.Normalize();
                        hasNormal = true;
                    }
                } else
                {
                    n.Normalize();
                }
            }

        }

        private class DataFetcherJob
        {
            public FetchDrawcall dc;
            public ListViewItem listItem;
        }

        private class TextureDescriptor
        {
            public readonly long fbxId;
            public ResourceId id;
            public FormatComponentType typeHint;


            public TextureDescriptor(ResourceId _id, FormatComponentType _typeHint)
            {
                fbxId = FbxGenerateId();
                id = _id;
                typeHint = _typeHint;
            }
        }

        private class DataFetcherRawData
        {
            public string name;
            public UInt32 eventId;
            public UInt32 numTris;
            public UInt32 numInstances;

            public Vec4 viewport;

            public FormatElement[] formatElements;
            public byte[][] rawVertexStreams;
            public UInt32[] vertexStreamSizeInVertices;
            public UInt32[] vertexStreamStrides;
            public UInt32[] indexBuffer;
            public UInt32 minIndex;
            public UInt32 maxIndex;
            public int baseVertex;

            public byte[][] rawPostVertexStreams;
            public UInt32[][] postIndexBuffers;
            public UInt32 postVertexStride;

            public bool isExportColor;
            public bool isExportDepth;

            public TextureDescriptor[] textures;
            public TextureDescriptor[] renderTargets;
            public TextureDescriptor depthBuffer;
        }

        private class AnalysisResult
        {
            public UInt32 instanceIndex;
            public DataFetcherResult fetchedData;

            public VertexEx[] baseVertices;
            public Vec3 baseVerticesBoundExt;

            public bool hasWorldViewProjection = false;
            public Mat4x4 worldViewProjection;
            public Mat4x4 invWorldViewProjection;

            public int projectionBucket = -1;
        }

        private class DataFetcherResult
        {
            public string name;
            public UInt32 eventId;
            public UInt32 numTris;
            public UInt32 numInstances;
            public Vec4 viewport;
            public VertexEx[][] vertexDataInstances;
            public UInt32[] indexBuffer;
            public TextureDescriptor[] textures;
            public TextureDescriptor[] renderTargets;
            public TextureDescriptor depthBuffer;
            public bool isExportColor;
            public bool isExportDepth;
            public bool hasLocalNormals;
            public UInt32 uiNumRenderTargets;
            public UInt32 uiNumTextures;
            public UInt32 uiWidth;
            public UInt32 uiHeight;

            public AnalysisResult[] analysisResults;

            public bool uiHasProjMatrix;
        }

        private enum FbxExportFilter
        {
            SKIP_DEPTH_ONLY = 0,
            INCLUDE_DEPTH_ONLY = 1,
            INCLUDE_COLOR_ONLY = 2,
            INCLUDE_ALL = 3,
        }

        private class FbxExportOptions
        {
            public bool exportTextures = true;
            public bool exportFrustums = true;
            public bool exportWorldTransforms = true;
            public bool exportVerticalSwapUV = true;
            public int uvIndex = 0;
            public int textureSlotIndex = 0;
            public FbxExportFilter filter = FbxExportFilter.SKIP_DEPTH_ONLY;
            public DataFetcherResult[] inputData;
        }



        private class DataFetcher
        {
            private ProgressBar m_Progress = null;
            private Control m_Parent = null;
            private Core m_Core = null;
            private ConcurrentQueue<DataFetcherJob> jobsInProgress = new ConcurrentQueue<DataFetcherJob>();
            private Thread fetchingThread;
            private ManualResetEvent allowedToWork = new ManualResetEvent(true);


            public DataFetcher(Core core, Control parent, ProgressBar progress)
            {
                m_Progress = progress;
                m_Core = core;
                m_Parent = parent;
            }

            private void ParserThreadMain()
            {
                while (true)
                {
                    allowedToWork.WaitOne();

                    int sleepTime = 0;

                    if (m_Core == null || m_Core.LogLoaded == false)
                    {
                        Thread.Sleep(1500);
                    }
                    else
                    {
                        DataFetcherJob job = null;
                        if (jobsInProgress.TryDequeue(out job))
                        {
                            ParserProcessEvent(job);
                            Thread.Sleep(sleepTime);
                        }
                        else
                        {
                            Thread.Sleep(1000);
                        }
                    }
                }
            }

            private void ParserProcessEvent(DataFetcherJob job)
            {
                CommonPipelineState exportPipelineState = new CommonPipelineState();

                DataFetcherRawData rawResult = new DataFetcherRawData();

                m_Core.Renderer.Invoke((ReplayRenderer r) =>
                {
                    FetchDrawcall dc = job.dc;

                    r.SetFrameEvent(dc.eventID, false);

                    rawResult.name = String.Copy(dc.name);
                    rawResult.eventId = dc.eventID;
                    rawResult.numTris = dc.numIndices / 3;

                    APIProperties apiProperties = r.GetAPIProperties();
                    D3D11PipelineState d3D11PipelineState = r.GetD3D11PipelineState();
                    D3D12PipelineState d3D12PipelineState = r.GetD3D12PipelineState();
                    GLPipelineState gLPipelineState = r.GetGLPipelineState();
                    VulkanPipelineState vulkanPipelineState = r.GetVulkanPipelineState();
                    exportPipelineState.SetStates(apiProperties, d3D11PipelineState, d3D12PipelineState, gLPipelineState, vulkanPipelineState);

                    Viewport viewport = exportPipelineState.GetViewport(0);
                    rawResult.viewport = new Vec4(viewport.x, viewport.y, viewport.width, viewport.height);

                    BoundResource depthOutput = exportPipelineState.GetDepthTarget();
                    rawResult.depthBuffer = new TextureDescriptor(depthOutput.Id, depthOutput.typeHint);

                    BoundResource[] outputTargets = exportPipelineState.GetOutputTargets();
                    rawResult.renderTargets = new TextureDescriptor[outputTargets.Length];
                    for(int i = 0; i < outputTargets.Length; i++)
                    {
                        ResourceId id = outputTargets[i].Id;
                        FormatComponentType typeHint = outputTargets[i].typeHint;
                        rawResult.renderTargets[i] = new TextureDescriptor(id, typeHint);
                    }

                    // 
                    rawResult.isExportColor = exportPipelineState.IsExportColor;
                    rawResult.isExportDepth = exportPipelineState.IsExportDepth;

                    // Get vertex inputs
                    VertexInputAttribute[] vinputs = exportPipelineState.GetVertexInputs();

                    // Convert vertex inputs to FormatElements
                    FormatElement[] formatElements = new FormatElement[vinputs.Length];
                    int numinputs = vinputs.Length;
                    int j = 0;
                    foreach (VertexInputAttribute vinput in vinputs)
                    {
                        if (!vinput.Used)
                        {
                            numinputs--;
                            Array.Resize(ref formatElements, numinputs);
                            continue;
                        }

                        formatElements[j] = new FormatElement(vinput.Name,
                                                              vinput.VertexBuffer,
                                                              vinput.RelativeByteOffset,
                                                              vinput.PerInstance,
                                                              vinput.InstanceRate,
                                                              false, // row major matrix
                                                              1, // matrix dimension
                                                              vinput.Format,
                                                              false);
                        j++;
                    }
                    rawResult.formatElements = formatElements;

                    // Get vertex streams
                    BoundVBuffer[] vbs = exportPipelineState.GetVBuffers();

                    byte[][] rawVertexStreams = new byte[vbs.Length][];
                    // Fetch raw preVS vertex data
                    for (int i = 0; i < vbs.Length; i++)
                    {
                        if (vbs[i].Buffer == ResourceId.Null)
                        {
                            rawVertexStreams[i] = null;
                            continue;
                        }
                        rawVertexStreams[i] = r.GetBufferData(vbs[i].Buffer, vbs[i].ByteOffset, 0);
                    }
                    rawResult.rawVertexStreams = rawVertexStreams;

                    // 
                    UInt32[] vertexStreamSizeInVertices = new UInt32[vbs.Length];
                    UInt32[] vertexStreamStrides = new UInt32[vbs.Length];
                    for (int i = 0; i < vbs.Length; i++)
                    {
                        UInt32 byteStride = vbs[i].ByteStride;
                        vertexStreamStrides[i] = byteStride;
                        if (rawVertexStreams[i] == null || byteStride == 0)
                        {
                            vertexStreamSizeInVertices[i] = 0;
                            continue;
                        }
                        vertexStreamSizeInVertices[i] = (UInt32)rawVertexStreams[i].Length / byteStride;
                    }
                    rawResult.vertexStreamSizeInVertices = vertexStreamSizeInVertices;
                    rawResult.vertexStreamStrides = vertexStreamStrides;

                    // Get index buffer
                    ResourceId ibuffer = ResourceId.Null;
                    ulong ioffset = 0;
                    exportPipelineState.GetIBuffer(out ibuffer, out ioffset);

                    // Fetch preVS index data
                    byte[] rawIndexBuffer = r.GetBufferData(ibuffer, ioffset + dc.indexOffset * dc.indexByteWidth, dc.numIndices * dc.indexByteWidth);

                    MemoryStream indexStream = new MemoryStream(rawIndexBuffer);
                    BinaryReader indexReader = new BinaryReader(indexStream);

                    rawResult.baseVertex = dc.baseVertex;
                    bool isIndexRestartEnabled = exportPipelineState.IsStripRestartEnabled();
                    UInt32 indexRestartValue = exportPipelineState.GetStripRestartIndex(dc != null ? dc.indexByteWidth : 0);

                    UInt32 minIndex = UInt32.MaxValue;
                    UInt32 maxIndex = UInt32.MinValue;
                    UInt32[] indexBuffer = new UInt32[rawIndexBuffer.Length / dc.indexByteWidth];

                    for (int i = 0; i < indexBuffer.Length; i++)
                    {
                        UInt32 index = 0;
                        if (dc.indexByteWidth == 1)
                        {
                            index = indexReader.ReadByte();
                        }
                        else if (dc.indexByteWidth == 2)
                        {
                            index = indexReader.ReadUInt16();
                        }
                        else if (dc.indexByteWidth == 4)
                        {
                            index = indexReader.ReadUInt32();
                        }
                        indexBuffer[i] = index;
                        minIndex = Math.Min(minIndex, index);
                        maxIndex = Math.Max(maxIndex, index);
                    }
                    rawResult.indexBuffer = indexBuffer;
                    rawResult.minIndex = minIndex;
                    rawResult.maxIndex = maxIndex;

                    // Fetch postVS data for each instance
                    UInt32 numInstances = dc.numInstances;
                    if (numInstances == 0 )
                    {
                        numInstances = 1;
                    }
                    rawResult.numInstances = numInstances;

                    rawResult.rawPostVertexStreams = new byte[numInstances][];
                    rawResult.postIndexBuffers = new UInt32[numInstances][];

                    for (uint instanceIndex = 0; instanceIndex < numInstances; instanceIndex++)
                    {
                        MeshFormat postVS = r.GetPostVSData(instanceIndex, MeshDataStage.VSOut);
                        byte[] rawPostIndexBuffer = r.GetBufferData(postVS.idxbuf, 0, 0);

                        // Fetch postVS index data
                        MemoryStream postIndexStream = new MemoryStream(rawPostIndexBuffer);
                        BinaryReader postIndexReader = new BinaryReader(postIndexStream);

                        UInt32 postMaxIndex = UInt32.MinValue;
                        UInt32[] postIndexBuffer = new UInt32[rawPostIndexBuffer.Length / dc.indexByteWidth];
                        for (int i = 0; i < postIndexBuffer.Length; i++)
                        {
                            UInt32 index = 0;
                            if (dc.indexByteWidth == 1)
                            {
                                index = postIndexReader.ReadByte();
                            }
                            else if (dc.indexByteWidth == 2)
                            {
                                index = postIndexReader.ReadUInt16();
                            }
                            else if (dc.indexByteWidth == 4)
                            {
                                index = postIndexReader.ReadUInt32();
                            }

                            postIndexBuffer[i] = index;
                            if (!isIndexRestartEnabled || index != indexRestartValue)
                            {
                                postMaxIndex = Math.Max(postMaxIndex, index);
                            }
                        }

                        // Fetch postVS vertex data
                        byte[] rawPostVertexStream = r.GetBufferData(postVS.buf, postVS.offset, (postMaxIndex + 1) * postVS.stride);

                        rawResult.rawPostVertexStreams[instanceIndex] = rawPostVertexStream;
                        rawResult.postIndexBuffers[instanceIndex] = postIndexBuffer;
                        rawResult.postVertexStride = postVS.stride;
                    }

                    // Fetch used textures
                    Dictionary<BindpointMap, BoundResource[]> fragmentReadOnlyResources = exportPipelineState.GetReadOnlyResources(ShaderStageType.Fragment);
                    ShaderBindpointMapping fragmentMapping = exportPipelineState.GetBindpointMapping(ShaderStageType.Fragment);

                    List<TextureDescriptor> textures = new List<TextureDescriptor>();
                    for (int idx = 0; idx < fragmentMapping.ReadOnlyResources.Length; idx++)
                    {
                        BindpointMap key = fragmentMapping.ReadOnlyResources[idx];
                        if (key.used == false)
                        {
                            continue;
                        }

                        BoundResource[] resArray = null;
                        if (fragmentReadOnlyResources.ContainsKey(key))
                        {
                            resArray = fragmentReadOnlyResources[key];
                        }

                        if (resArray == null)
                        {
                            continue;
                        }

                        int count = resArray.Length;
                        if (count > 1)
                        {
                            count = 1;
                        }

                        for (int i = 0; i < count; i++)
                        {
                            ResourceId id = resArray[i].Id;
                            FormatComponentType typeHint = resArray[i].typeHint;
                            textures.Add(new TextureDescriptor(id, typeHint));
                        }
                    }

                    rawResult.textures = textures.ToArray();
                });


                DataFetcherResult res = ParseRawData(rawResult);
                if (res == null)
                {
                    //Update UI from main thread
                    m_Parent.BeginInvoke(new Action(() =>
                    {
                        ListItemData data = job.listItem.Tag as ListItemData;
                        data.fetchResult = null;
                        job.listItem.SubItems[13].Text = "Error";
                    }));
                }
                else
                {

                    AnalyzeFetchedData(res);

                    //prepare UI data
                    res.uiNumRenderTargets = 0;
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < res.renderTargets.Length; i++)
                    {
                        if (res.renderTargets[i].id != ResourceId.Null)
                        {
                            if (res.uiNumRenderTargets > 0)
                            {
                                sb.Append(", ");
                            }
                            res.uiNumRenderTargets++;
                            sb.Append(res.renderTargets[i].id.ToString());
                        }
                    }


                    res.uiNumTextures = 0;
                    for (int i = 0; i < res.textures.Length; i++)
                    {
                        if (res.textures[i].id != ResourceId.Null)
                        {
                            res.uiNumTextures++;
                        }
                    }

                    res.uiWidth = (UInt32)Math.Abs(res.viewport.z - res.viewport.x);
                    res.uiHeight = (UInt32)Math.Abs(res.viewport.w - res.viewport.y);

                    res.uiHasProjMatrix = false;

                    if (res.analysisResults != null)
                    {
                        for (UInt32 instanceId = 0; instanceId < res.numInstances; instanceId++)
                        {
                            if (res.analysisResults[instanceId] == null)
                            {
                                continue;
                            }

                            if (res.analysisResults[instanceId].hasWorldViewProjection)
                            {
                                res.uiHasProjMatrix = true;
                                break;
                            }
                        }
                    }


                    //Update UI from main thread
                    m_Parent.BeginInvoke(new Action(() =>
                    {
                        ListItemData data = job.listItem.Tag as ListItemData;
                        data.fetchResult = res;
                        job.listItem.SubItems[4].Text = res.uiNumRenderTargets.ToString();
                        job.listItem.SubItems[5].Text = res.depthBuffer.id.ToString();
                        job.listItem.SubItems[6].Text = sb.ToString();
                        job.listItem.SubItems[7].Text = res.isExportColor ? "+" : "";
                        job.listItem.SubItems[8].Text = res.uiWidth.ToString();
                        job.listItem.SubItems[9].Text = res.uiHeight.ToString();
                        job.listItem.SubItems[10].Text = res.uiNumTextures.ToString();
                        job.listItem.SubItems[11].Text = res.uiHasProjMatrix ? "+" : "";
                        job.listItem.SubItems[12].Text = res.hasLocalNormals ? "+" : "";
                        job.listItem.SubItems[13].Text = "Ready to Export";

                        int progressValue = m_Progress.Value;

                        if (m_Progress.Value >= m_Progress.Minimum && m_Progress.Value < m_Progress.Maximum)
                        {
                            m_Progress.Value = progressValue + 1;
                            if (m_Progress.Value == m_Progress.Maximum)
                            {
                                m_Progress.Hide();
                            }
                        }
                    }));

                }
                

            }

            private void AnalyzeFetchedData(DataFetcherResult data)
            {
                data.analysisResults = new AnalysisResult[data.numInstances];

                for (UInt32 instanceId = 0; instanceId < data.numInstances; instanceId++)
                {
                    AnalysisResult analysisResult = new AnalysisResult();
                    analysisResult.instanceIndex = instanceId;
                    analysisResult.fetchedData = data;
                    data.analysisResults[instanceId] = analysisResult;

                    // Find 4 tetrahedron vertices our basement to reconstruct the world-view-projection matrix
                    /////////////////////////////////////////////////////////////////////////////////////////////////

                    VertexEx[] vertexData = data.vertexDataInstances[instanceId];

                    if (vertexData.Length < 4)
                    {
                        // Not enough vertices
                        continue;
                    }

                    // Find the bounding box
                    Vec3 bbMin = vertexData[0].p;
                    Vec3 bbMax = vertexData[0].p;
                    for (int i = 1; i < vertexData.Length; i++)
                    {
                        if (vertexData[i] == null)
                        {
                            continue;
                        }
                        bbMin = Vec3.Min(bbMin, vertexData[i].p);
                        bbMax = Vec3.Max(bbMax, vertexData[i].p);
                    }

                    Vec3 bbExt = (bbMax - bbMin);
                    if (bbExt.x < 0.00001 || bbExt.y < 0.00001 || bbExt.z < 0.00001)
                    {
                        //Bounding box does not has a volume
                        continue;
                    }

                    VertexEx[] baseVertices = new VertexEx[4];

                    // First vertex is simple, just find the vertex nearest to bound box corner
                    Vec3[] bbCorners = new Vec3[8];
                    bbCorners[0] = new Vec3(bbMin.x, bbMin.y, bbMin.z);
                    bbCorners[1] = new Vec3(bbMax.x, bbMin.y, bbMin.z);
                    bbCorners[2] = new Vec3(bbMin.x, bbMin.y, bbMax.z);
                    bbCorners[3] = new Vec3(bbMax.x, bbMin.y, bbMax.z);
                    bbCorners[4] = new Vec3(bbMin.x, bbMax.y, bbMin.z);
                    bbCorners[5] = new Vec3(bbMax.x, bbMax.y, bbMin.z);
                    bbCorners[6] = new Vec3(bbMin.x, bbMax.y, bbMax.z);
                    bbCorners[7] = new Vec3(bbMax.x, bbMax.y, bbMax.z);

                    int nearestIndex = -1;
                    double sqLen = double.MaxValue;
                    for (int i = 0; i < vertexData.Length; i++)
                    {
                        if (vertexData[i] == null)
                        {
                            continue;
                        }

                        for (int j = 0; j < bbCorners.Length; j++)
                        {
                            double sqLenCur = (vertexData[i].p - bbCorners[j]).SqLength();
                            if (sqLenCur < sqLen)
                            {
                                sqLen = sqLenCur;
                                nearestIndex = i;
                            }
                        }
                    }

                    // Can't find any vertex
                    if (nearestIndex < 0)
                    {
                        continue;
                    }
                    baseVertices[0] = vertexData[nearestIndex];


                    // Second is simple too, just find the furthest vertex from the first one
                    int furthestIndex = -1;
                    sqLen = 0.0f;
                    for (int i = 1; i < vertexData.Length; i++)
                    {
                        if (vertexData[i] == null)
                        {
                            continue;
                        }

                        double sqLenCur = (vertexData[i].p - baseVertices[0].p).SqLength();
                        if (sqLenCur > sqLen)
                        {
                            sqLen = sqLenCur;
                            furthestIndex = i;
                        }
                    }

                    // Can't find vertex
                    if (furthestIndex < 0)
                    {
                        continue;
                    }
                    baseVertices[1] = vertexData[furthestIndex];


                    //Third vertex is more complex, find the furthest vertex from the line formed by the first and second vertice
                    Vec3 lineOrigin = baseVertices[0].p;
                    Vec3 lineDirection = (baseVertices[1].p - baseVertices[0].p);
                    lineDirection.Normalize();

                    furthestIndex = -1;
                    sqLen = 0.0f;
                    for (int i = 1; i < vertexData.Length; i++)
                    {
                        if (vertexData[i] == null)
                        {
                            continue;
                        }

                        Vec3 diff = vertexData[i].p - lineOrigin;
                        double p = Vec3.Dot(lineDirection, diff);

                        Vec3 cpoint = lineOrigin;
                        if (p > 0.0f)
                        {
                            cpoint = lineOrigin + lineDirection * p;
                        }

                        double sqLenCur = (cpoint - vertexData[i].p).SqLength();
                        if (sqLenCur > sqLen)
                        {
                            sqLen = sqLenCur;
                            furthestIndex = i;
                        }
                    }

                    // Can't find vertex
                    if (furthestIndex < 0)
                    {
                        continue;
                    }
                    baseVertices[2] = vertexData[furthestIndex];


                    //Fourth vertex is complex too, find the furthest vertex from the plane formed by the first and second and third vertex
                    Vec3 e1 = baseVertices[1].p - baseVertices[0].p;
                    Vec3 e2 = baseVertices[2].p - baseVertices[0].p;
                    e1.Normalize();
                    e2.Normalize();

                    Vec3 planeN = Vec3.Cross(e1, e2);
                    double planeD = Vec3.Dot(baseVertices[0].p, planeN);

                    furthestIndex = -1;
                    sqLen = 0.0f;
                    for (int i = 1; i < vertexData.Length; i++)
                    {
                        if (vertexData[i] == null)
                        {
                            continue;
                        }

                        double signedD = Vec3.Dot(planeN, vertexData[i].p) - planeD;
                        double sqLenCur = signedD * signedD;

                        if (sqLenCur > sqLen)
                        {
                            sqLen = sqLenCur;
                            furthestIndex = i;
                        }
                    }
                    // Can't find vertex
                    if (furthestIndex < 0)
                    {
                        continue;
                    }
                    baseVertices[3] = vertexData[furthestIndex];


                    // Find the bounding box of base vertices
                    /////////////////////////////////////////////////////////////////////////////////////////////////
                    Vec3 bbTethraMin = baseVertices[0].p;
                    Vec3 bbTethraMax = baseVertices[0].p;
                    for (int i = 1; i < baseVertices.Length; i++)
                    {
                        bbTethraMin = Vec3.Min(bbTethraMin, baseVertices[i].p);
                        bbTethraMax = Vec3.Max(bbTethraMax, baseVertices[i].p);
                    }

                    Vec3 bbTethraExt = (bbTethraMax - bbTethraMin);
                    


                    // Reconstruct world-view-projection matrix
                    /////////////////////////////////////////////////////////////////////////////////////////////////

                    // Linear equations system solver
                    Vec4 input0 = new Vec4(baseVertices[0].pp.x, baseVertices[1].pp.x, baseVertices[2].pp.x, baseVertices[3].pp.x);
                    Vec4 input1 = new Vec4(baseVertices[0].pp.y, baseVertices[1].pp.y, baseVertices[2].pp.y, baseVertices[3].pp.y);
                    Vec4 input2 = new Vec4(baseVertices[0].pp.z, baseVertices[1].pp.z, baseVertices[2].pp.z, baseVertices[3].pp.z);
                    Vec4 input3 = new Vec4(baseVertices[0].pp.w, baseVertices[1].pp.w, baseVertices[2].pp.w, baseVertices[3].pp.w);

                    Mat4x4 transform = new Mat4x4(
                        baseVertices[0].p.x, baseVertices[0].p.y, baseVertices[0].p.z, 1.0f,
                        baseVertices[1].p.x, baseVertices[1].p.y, baseVertices[1].p.z, 1.0f,
                        baseVertices[2].p.x, baseVertices[2].p.y, baseVertices[2].p.z, 1.0f,
                        baseVertices[3].p.x, baseVertices[3].p.y, baseVertices[3].p.z, 1.0f);

                    Mat4x4 iTransform = transform;
                    bool invRes = iTransform.Inverse();
                    if (invRes == false)
                    {
                        //Can't solve linear equations system
                        continue;
                    }

                    Vec4 r0 = iTransform * input0; // 00, 01, 02, 03
                    Vec4 r1 = iTransform * input1; // 10, 11, 12, 13
                    Vec4 r2 = iTransform * input2; // 20, 21, 22, 23
                    Vec4 r3 = iTransform * input3; // 30, 31, 32, 33

                    Mat4x4 worldViewProjection = new Mat4x4( r0.x, r0.y, r0.z, r0.w,
                                                             r1.x, r1.y, r1.z, r1.w,
                                                             r2.x, r2.y, r2.z, r2.w,
                                                             r3.x, r3.y, r3.z, r3.w);


                    Mat4x4 invWorldViewProjection = worldViewProjection;
                    if (!invWorldViewProjection.Inverse())
                    {
                        continue;
                    }


                    // Validate founded transform
                    //
                    // Linear equations system can be solved properly
                    // But the resulting matrix may not translate vertices into a post perspective space and vice versa
                    // Suppose some kind of procedural vertex animation or skining is used
                    //

                    //0.5% from local bound box is good epsilon to validate transform matrix
                    Vec3 checkEps = bbExt * 0.005f;
                    double wEps = 0.05;
                    bool wvpCheckFailed = false;
                    for(int i = 0; i < vertexData.Length; i++)
                    {
                        if (vertexData[i] == null)
                        {
                            continue;
                        }

                        Vec4 local4 = (invWorldViewProjection * vertexData[i].pp);
                        if (Math.Abs(local4.w - 1.0) > wEps)
                        {
                            // w must but equal to 1.0
                            wvpCheckFailed = true;
                            break;
                        }

                        Vec3 local3 = local4.HomogeneousToCartesian();
                        if (!Vec3.IsNearEqual(vertexData[i].p, local3, checkEps))
                        {
                            wvpCheckFailed = true;
                            break;
                        }
                    }

                    if (wvpCheckFailed)
                    {
                        continue;
                    }

                    analysisResult.baseVertices = baseVertices;
                    analysisResult.baseVerticesBoundExt = bbTethraExt;
                    analysisResult.hasWorldViewProjection = true;
                    analysisResult.worldViewProjection = worldViewProjection;
                    analysisResult.invWorldViewProjection = invWorldViewProjection;

                } // instances iterator

            }

            private DataFetcherResult ParseRawData(DataFetcherRawData data)
            {
                UInt32 verticesCount = (data.maxIndex - data.minIndex) + 1;
                if (verticesCount == 0)
                {
                    return null;
                }

                UInt32 numInstances = data.numInstances;
                for (int i = 0; i < numInstances; i++)
                {
                    if (data.indexBuffer.Length != data.postIndexBuffers[i].Length)
                    {
                        //New triangles was added or removed. GS is enabled?
                        return null;
                    }
                }

                int vertexStreamsCount = data.rawVertexStreams.Length;

                // Create streams and readers
                // preVS
                Stream[] vbStreams = new Stream[vertexStreamsCount];
                BinaryReader[] vbReaders = new BinaryReader[vertexStreamsCount];
                for(int i = 0; i < vertexStreamsCount; i++)
                {
                    byte[] rawVertexStream = data.rawVertexStreams[i];
                    if (rawVertexStream != null)
                    {
                        vbStreams[i] = new MemoryStream(rawVertexStream);
                        vbReaders[i] = new BinaryReader(vbStreams[i]);
                    }
                    else
                    {
                        vbStreams[i] = null;
                        vbReaders[i] = null;
                    }
                }

                // postVS
                Stream[] postStreams = new Stream[numInstances];
                BinaryReader[] postReaders = new BinaryReader[numInstances];

                for (uint i = 0; i < numInstances; i++)
                {
                    byte[] rawPostVertexStream = data.rawPostVertexStreams[i];
                    if (rawPostVertexStream != null)
                    {
                        postStreams[i] = new MemoryStream(rawPostVertexStream);
                        postReaders[i] = new BinaryReader(postStreams[i]);
                    }
                    else
                    {
                        postStreams[i] = null;
                        postReaders[i] = null;
                    }
                }


                DataFetcherResult result = new DataFetcherResult();

                result.name = data.name;
                result.eventId = data.eventId;
                result.numTris = data.numTris;
                result.numInstances = data.numInstances;
                result.viewport = data.viewport;
                result.indexBuffer = new UInt32[data.indexBuffer.Length];
                result.textures = data.textures;
                result.renderTargets = data.renderTargets;
                result.depthBuffer = data.depthBuffer;
                result.isExportColor = data.isExportColor;
                result.isExportDepth = data.isExportDepth;
                result.vertexDataInstances = new VertexEx[numInstances][];
                result.hasLocalNormals = true;

                for (uint instanceIndex = 0; instanceIndex < numInstances; instanceIndex++)
                {
                    VertexEx[] vertexData = new VertexEx[verticesCount];
                    result.vertexDataInstances[instanceIndex] = vertexData;

                    for (uint i = 0; i < data.indexBuffer.Length; i++)
                    {
                        UInt32 indice = data.indexBuffer[i];

                        UInt32 vertexIndex = indice - data.minIndex;
                        result.indexBuffer[i] = vertexIndex;

                        // Apply base vertex
                        if (data.baseVertex < 0)
                        {
                            uint subtract = (uint)(-data.baseVertex);
                            if (indice < subtract)
                                indice = 0;
                            else
                                indice -= subtract;
                        }
                        else if (data.baseVertex > 0)
                        {
                            indice += (uint)data.baseVertex;
                        }

                        VertexEx vertex = new VertexEx();

                        for (int el = 0; el < data.formatElements.Length; el++)
                        {
                            int vertexStreamIndex = data.formatElements[el].buffer;
                            if (vertexStreamIndex >= vertexStreamsCount)
                            {
                                continue;
                            }

                            byte[] rawVertexStream = data.rawVertexStreams[vertexStreamIndex];
                            Stream vbStream = vbStreams[vertexStreamIndex];
                            BinaryReader vbReader = vbReaders[vertexStreamIndex];
                            UInt32 stride = data.vertexStreamStrides[vertexStreamIndex];

                            if (rawVertexStream == null || vbStream == null || vbReader == null || stride == 0)
                            {
                                continue;
                            }

                            uint curIndice = indice;
                            // Handle instance rate being 0 (every instance takes index 0 in that case)
                            if (data.formatElements[el].perinstance)
                            {
                                curIndice = data.formatElements[el].instancerate > 0 ? (instanceIndex / (uint)data.formatElements[el].instancerate) : 0;
                            }
                            uint bytesOffset = stride * indice + data.formatElements[el].offset;
                            if (bytesOffset >= rawVertexStream.Length)
                            {
                                //ERROR: out of bounds
                                continue;
                            }
                            else
                            {
                                vbStream.Seek(bytesOffset, SeekOrigin.Begin);
                            }

                            UInt32 elementByteSize = data.formatElements[el].ByteSize;
                            byte[] elementBytes = vbReader.ReadBytes((int)elementByteSize);
                            if (elementBytes.Length != elementByteSize)
                            {
                                //ERROR: out of bounds
                                continue;
                            }

                            // Unpack vertex data
                            object[] elements = null;
                            {
                                MemoryStream tempStream = new MemoryStream(elementBytes);
                                BinaryReader tempReader = new BinaryReader(tempStream);
                                elements = data.formatElements[el].GetObjects(tempReader);
                                tempReader.Dispose();
                                tempStream.Dispose();
                            }

                            // Convert to double

                            double[] dblElements = new double[elements.Length];
                            for (int j = 0; j < elements.Length; j++)
                            {
                                object o = elements[j];
                                double val;
                                if (o is uint)
                                    val = (double)(uint)elements[j];
                                else if (o is int)
                                    val = (double)(int)elements[j];
                                else if (o is UInt16)
                                    val = (double)(int)elements[j];
                                else if (o is Int16)
                                    val = (double)(int)elements[j];
                                else if (o is float)
                                    val = (double)(float)elements[j];
                                else
                                    val = (double)elements[j];

                                dblElements[j] = val;
                            }

                            /////////////////////////////////////////////////////////
                            if (el == 0 || (data.formatElements[el].offset == 0 && data.formatElements[el].buffer == 0))
                            {
                                if (dblElements.Length > 0)
                                    vertex.e0.x = dblElements[0];

                                if (dblElements.Length > 1)
                                    vertex.e0.y = dblElements[1];

                                if (dblElements.Length > 2)
                                    vertex.e0.z = dblElements[2];
                            }

                            /////////////////////////////////////////////////////////
                            if (data.formatElements[el].name.ToUpperInvariant() == "POSITION" ||
                                data.formatElements[el].name.ToUpperInvariant() == "POSITION0" ||
                                data.formatElements[el].name.ToUpperInvariant() == "POS" ||
                                data.formatElements[el].name.ToUpperInvariant() == "POS0" ||
                                (data.formatElements[el].name.ToUpperInvariant().Contains("POSITION") && vertex.hasPosition == false) ||
                                (data.formatElements[el].name.ToUpperInvariant().Contains("POS") && vertex.hasPosition == false))
                            {
                                if (dblElements.Length > 0)
                                    vertex.p.x = dblElements[0];

                                if (dblElements.Length > 1)
                                    vertex.p.y = dblElements[1];

                                if (dblElements.Length > 2)
                                    vertex.p.z = dblElements[2];

                                vertex.hasPosition = true;
                            }

                            /////////////////////////////////////////////////////////
                            if (data.formatElements[el].name.ToUpperInvariant() == "NORMAL" ||
                                data.formatElements[el].name.ToUpperInvariant() == "NORMAL0" ||
                                (data.formatElements[el].name.ToUpperInvariant().Contains("NORM") &&
                                 data.formatElements[el].name.ToUpperInvariant() != "BINORMAL" &&
                                 vertex.hasNormal == false))
                            {
                                //custom unpack for UInt32
                                if (elements.Length == 1 && data.formatElements[el].format.compType == FormatComponentType.UInt &&
                                    data.formatElements[el].format.compByteWidth == 4)
                                {
                                    UInt32 v = (UInt32)elements[0];
                                    byte x = (byte)(v & 0xFF);
                                    byte y = (byte)((v >> 8) & 0xFF);
                                    byte z = (byte)((v >> 16) & 0xFF);

                                    double nx = x / 255.0;
                                    double ny = y / 255.0;
                                    double nz = z / 255.0;

                                    nx = nx * 2.0 - 1.0;
                                    ny = ny * 2.0 - 1.0;
                                    nz = nz * 2.0 - 1.0;

                                    vertex.n.x = nx;
                                    vertex.n.y = ny;
                                    vertex.n.z = nz;

                                } else if (elements.Length == 4 && data.formatElements[el].format.compType == FormatComponentType.UNorm)
                                {
                                    if (dblElements.Length > 0)
                                        vertex.n.x = dblElements[0] * 2.0 - 1.0;

                                    if (dblElements.Length > 1)
                                        vertex.n.y = dblElements[1] * 2.0 - 1.0;

                                    if (dblElements.Length > 2)
                                        vertex.n.z = dblElements[2] * 2.0 - 1.0;
                                }
                                else
                                {
                                    if (dblElements.Length > 0)
                                        vertex.n.x = dblElements[0];

                                    if (dblElements.Length > 1)
                                        vertex.n.y = dblElements[1];

                                    if (dblElements.Length > 2)
                                        vertex.n.z = dblElements[2];
                                }

                                vertex.hasNormal = true;
                            }

                            /////////////////////////////////////////////////////////
                            if (data.formatElements[el].name.ToUpperInvariant() == "TANGENT")
                            {
                                if (dblElements.Length > 0)
                                    vertex.t.x = dblElements[0];

                                if (dblElements.Length > 1)
                                    vertex.t.y = dblElements[1];

                                if (dblElements.Length > 2)
                                    vertex.t.z = dblElements[2];

                                if (dblElements.Length > 3)
                                    vertex.t.w = dblElements[3];
                            }

                            /////////////////////////////////////////////////////////
                            if (data.formatElements[el].name.ToUpperInvariant() == "BINORMAL")
                            {
                                if (dblElements.Length > 0)
                                    vertex.b.x = dblElements[0];

                                if (dblElements.Length > 1)
                                    vertex.b.y = dblElements[1];

                                if (dblElements.Length > 2)
                                    vertex.b.z = dblElements[2];

                                if (dblElements.Length > 3)
                                    vertex.b.w = dblElements[3];
                            }

                            /////////////////////////////////////////////////////////
                            if (data.formatElements[el].name.ToUpperInvariant() == "COLOR" ||
                                data.formatElements[el].name.ToUpperInvariant() == "COLOR0")
                            {
                                if (dblElements.Length > 0)
                                    vertex.clr.x = dblElements[0];

                                if (dblElements.Length > 1)
                                    vertex.clr.y = dblElements[1];

                                if (dblElements.Length > 2)
                                    vertex.clr.z = dblElements[2];

                                if (dblElements.Length > 3)
                                    vertex.clr.w = dblElements[3];
                            }

                            /////////////////////////////////////////////////////////
                            if (data.formatElements[el].name.ToUpperInvariant() == "TEXCOORD" ||
                                data.formatElements[el].name.ToUpperInvariant() == "TEXCOORD0" ||
                                (data.formatElements[el].name.ToUpperInvariant().Contains("TEXCOORD") && vertex.hasUV == false))
                            {
                                if (dblElements.Length > 0)
                                    vertex.uv0.x = dblElements[0];

                                if (dblElements.Length > 1)
                                    vertex.uv0.y = dblElements[1];

                                vertex.hasUV = true;
                            }

                            /////////////////////////////////////////////////////////
                            if (data.formatElements[el].name.ToUpperInvariant() == "TEXCOORD1")
                            {
                                if (dblElements.Length > 0)
                                    vertex.uv1.x = dblElements[0];

                                if (dblElements.Length > 1)
                                    vertex.uv1.y = dblElements[1];
                            }

                            /////////////////////////////////////////////////////////
                            if (data.formatElements[el].name.ToUpperInvariant() == "TEXCOORD2")
                            {
                                if (dblElements.Length > 0)
                                    vertex.uv2.x = dblElements[0];

                                if (dblElements.Length > 1)
                                    vertex.uv2.y = dblElements[1];
                            }

                            /////////////////////////////////////////////////////////
                            if (data.formatElements[el].name.ToUpperInvariant() == "TEXCOORD3")
                            {
                                if (dblElements.Length > 0)
                                    vertex.uv3.x = dblElements[0];

                                if (dblElements.Length > 1)
                                    vertex.uv3.y = dblElements[1];
                            }


                            /////////////////////////////////////////////////////////
                            if (data.formatElements[el].name.ToUpperInvariant() == "BLENDWEIGHT")
                            {
                                if (dblElements.Length > 0)
                                    vertex.blendWeight.x = dblElements[0];

                                if (dblElements.Length > 1)
                                    vertex.blendWeight.y = dblElements[1];

                                if (dblElements.Length > 2)
                                    vertex.blendWeight.z = dblElements[2];

                                if (dblElements.Length > 3)
                                    vertex.blendWeight.w = dblElements[3];
                            }

                            /////////////////////////////////////////////////////////
                            if (data.formatElements[el].name.ToUpperInvariant() == "BLENDINDICES")
                            {
                                if (dblElements.Length > 0)
                                    vertex.blendIndices.x = dblElements[0];

                                if (dblElements.Length > 1)
                                    vertex.blendIndices.y = dblElements[1];

                                if (dblElements.Length > 2)
                                    vertex.blendIndices.z = dblElements[2];

                                if (dblElements.Length > 3)
                                    vertex.blendIndices.w = dblElements[3];
                            }

                        } // elements iterator


                        Stream postStream = postStreams[instanceIndex];
                        BinaryReader postReader = postReaders[instanceIndex];
                        byte[] rawPostVertexStream = data.rawPostVertexStreams[instanceIndex];
                        UInt32[] postIndexBuffer = data.postIndexBuffers[instanceIndex];
                        UInt32 postIndice = postIndexBuffer[i];
                        UInt32 postVertexStride = data.postVertexStride;

                        // Assume SV_Position is always first Vec4 inside output stream
                        UInt32 svPositionOffset = 0;
                        UInt32 svPositionSizeOf = 16;

                        uint postVertexByteOffset = postVertexStride * postIndice + svPositionOffset;

                        if (postVertexByteOffset + svPositionSizeOf <= rawPostVertexStream.Length)
                        {
                            postStream.Seek(postVertexByteOffset, SeekOrigin.Begin);
                            vertex.pp.x = postReader.ReadSingle();
                            vertex.pp.y = postReader.ReadSingle();
                            vertex.pp.z = postReader.ReadSingle();
                            vertex.pp.w = postReader.ReadSingle();
                        }
                        else
                        {
                            //ERROR: out of bounds
                            vertex.pp.x = 0.0f;
                            vertex.pp.y = 0.0f;
                            vertex.pp.z = 0.0f;
                            vertex.pp.w = 0.0f;
                        }

                        vertex.Finish();
                        vertexData[vertexIndex] = vertex;
                        if (vertex.hasNormal == false)
                        {
                            result.hasLocalNormals = false;
                        }

                    } // indices iterator

                } // instance iterator


                return result;
            }


            public void Start()
            {
                // run parser thread
                fetchingThread = Helpers.NewThread(new ThreadStart(() =>
                {
                    ParserThreadMain();
                }));
                fetchingThread.IsBackground = true;
                fetchingThread.Start();
            }

            public void Stop()
            {
                //abort parsing thread
                if (fetchingThread != null &&
                    fetchingThread.ThreadState != ThreadState.Aborted &&
                    fetchingThread.ThreadState != ThreadState.Stopped)
                {
                    fetchingThread.Abort();
                    fetchingThread.Join();
                }

                fetchingThread = null;
            }

            public void Add(DataFetcherJob job)
            {
                jobsInProgress.Enqueue(job);
            }

            public void SetPause(bool isPaused)
            {
                if (isPaused)
                {
                    allowedToWork.Reset();
                } else
                {
                    allowedToWork.Set();
                }
            }
        }



        private class ListItemData
        {
            public string name;
            public UInt32 eventId;
            public UInt32 numTris;
            public UInt32 numInstances;
            public DataFetcherResult fetchResult = null;
        }

        class DrawCallsListViewItemComparer : IComparer
        {
            private int column;
            private SortOrder order;

            public DrawCallsListViewItemComparer()
            {
                column = 0;
            }

            public void SetColumn(int _column)
            {
                column = _column;
            }

            public void SetSortOrder(SortOrder _order)
            {
                order = _order;
            }

            public int GetColumn()
            {
                return column;
            }

            public SortOrder GetSortOrder()
            {
                return order;
            }

            private int InternalCompare(object a, object b)
            {
                ListViewItem itemA = (ListViewItem)a;
                ListViewItem itemB = (ListViewItem)b;
                ListItemData dataA = itemA.Tag as ListItemData;
                ListItemData dataB = itemB.Tag as ListItemData;

                DataFetcherResult fetchA = dataA.fetchResult;
                DataFetcherResult fetchB = dataB.fetchResult;

                if (column >= 4)
                {
                    if (fetchA == null && fetchB == null)
                        return 0;
                    if (fetchA == null && fetchB != null)
                        return -1;
                    if (fetchA != null && fetchB == null)
                        return 1;
                }

                switch (column)
                {
                    case 0:
                        return dataA.eventId.CompareTo(dataB.eventId);
                    case 1:
                        return dataA.name.CompareTo(dataB.name);
                    case 2:
                        return dataA.numTris.CompareTo(dataB.numTris);
                    case 3:
                        return dataA.numInstances.CompareTo(dataB.numInstances);
                    case 4:
                        return fetchA.uiNumRenderTargets.CompareTo(fetchB.uiNumRenderTargets);
                    case 5:
                    case 6:
                    case 7:
                    case 11:
                    case 12:
                    case 13:
                        return String.Compare(itemA.SubItems[column].Text, itemB.SubItems[column].Text);
                    case 8:
                        return fetchA.uiWidth.CompareTo(fetchB.uiWidth);
                    case 9:
                        return fetchA.uiHeight.CompareTo(fetchB.uiHeight);
                    case 10:
                        return fetchA.uiNumTextures.CompareTo(fetchB.uiNumTextures);
                }

                return 0;
            }

            public int Compare(object a, object b)
            {
                int v = InternalCompare(a, b);
                if (order == SortOrder.Descending)
                    v *= -1;

                return v;
            }
        }


        HashSet<UInt32> selectedDrawcalls = new HashSet<uint>();

        private UInt32 selectedEventFrom = 0;
        private UInt32 selectedEventTo = 0;
        private Core m_Core;
        private DataFetcher dataFetcher = null;
        private DrawCallsListViewItemComparer drawCallsSorter = new DrawCallsListViewItemComparer();
        private ProgressPopup modalExportProgress = null;

        public ExporterView(Core core)
        {
            InitializeComponent();
            uvChannelToExport.SelectedIndex = 0;
            texSlotToExport.SelectedIndex = 0;
            typeToExport.SelectedIndex = 0;

            Icon = global::renderdocui.Properties.Resources.icon;

            m_Core = core;

            if (m_Core.CurGLPipelineState != null || m_Core.CurGLPipelineState != null)
            {
                exportSwapUV.Checked = true;
            } else
            {
                exportSwapUV.Checked = false;
            }

            dataFetcher = new DataFetcher(m_Core, this, analysisProgress);

            drawcallsToExport.ListViewItemSorter = drawCallsSorter;

            UpdateStatus();
        }

        public void OnLogfileClosed()
        {
            drawcallsToExport.Items.Clear();
            selectedDrawcalls.Clear();
            clearDrawCalls.Enabled = false;
            removeSelected.Enabled = false;
            synchronize.Enabled = false;
            synchronize.Text = "Select EID";
            UpdateStatus();

            dataFetcher.Stop();

            addDrawCallsButton.Enabled = false;
        }

        public void OnLogfileLoaded()
        {
            if (m_Core.CurGLPipelineState != null || m_Core.CurGLPipelineState != null)
            {
                exportSwapUV.Checked = true;
            } else
            {
                exportSwapUV.Checked = false;
            }

            drawcallsToExport.Items.Clear();
            selectedDrawcalls.Clear();
            removeSelected.Enabled = false;
            synchronize.Enabled = false;
            clearDrawCalls.Enabled = false;
            synchronize.Text = "Select EID";
            UpdateStatus();

            dataFetcher.Start();
        }

        public void AddEventRange(UInt32 eventFrom, UInt32 eventTo)
        {
            AddDrawCalls(eventFrom, eventTo);
        }


        public void SetActiveEventRange(UInt32 eventFrom, UInt32 eventTo)
        {
            if (IsDisposed)
            {
                return;
            }

            selectedEventFrom = eventFrom;
            selectedEventTo = eventTo;

            addDrawCallsButton.Text = string.Format("Add EID ({0} - {1}) to FBX Exporter", selectedEventFrom, selectedEventTo);
            addDrawCallsButton.Enabled = true;
        }

        public void OnEventSelected(UInt32 eventID)
        {
            if (eventID >= selectedEventFrom && eventID <= selectedEventTo)
            {
                return;
            }
            SetActiveEventRange(eventID, eventID);
        }

        private void ExporterView_Load(object sender, EventArgs e)
        {

        }

        private void UpdateStatus()
        {
            UInt64 triCount = 0;
            UInt32 objectsCount = 0;

            int readyToExportCount = 0;

            foreach(ListViewItem item in drawcallsToExport.Items)
            {
                ListItemData data = item.Tag as ListItemData;
                triCount += data.numTris * data.numInstances;
                objectsCount++;

                if (data.fetchResult != null)
                {
                    readyToExportCount++;
                }
            }

            lblCount.Text = objectsCount.ToString();
            lblTriCount.Text = triCount.ToString("# ### ### ### ###");

            analysisProgress.Minimum = 0;
            analysisProgress.Maximum = drawcallsToExport.Items.Count;
            analysisProgress.Value = readyToExportCount;
            if (readyToExportCount == analysisProgress.Maximum)
            {
                analysisProgress.Hide();
            } else
            {
                analysisProgress.Show();
            }

            if (drawcallsToExport.Items.Count == 0)
            {
                clearDrawCalls.Enabled = false;
            } else
            {
                clearDrawCalls.Enabled = true;
            }
        }

        private void AddDrawCalls(UInt32 eventFrom, UInt32 eventTo)
        {
            bool needUpdateStatus = false;

            for (UInt32 eventId = eventFrom; eventId <= eventTo; eventId++)
            {
                FetchDrawcall dc = m_Core.GetDrawcall(eventId);
                if (dc == null)
                {
                    continue;
                }

                if (((dc.flags & DrawcallFlags.Drawcall) == 0) || ((dc.flags & DrawcallFlags.UseIBuffer) == 0))
                {
                    continue;
                }

                if (dc.topology != PrimitiveTopology.TriangleList)
                {
                    continue;
                }

                if (selectedDrawcalls.Add(eventId))
                {
                    UInt32 numInstances = dc.numInstances;
                    if (numInstances == 0)
                    {
                        numInstances = 1;
                    }

                    UInt32 numTris = dc.numIndices / 3;

                    ListItemData data = new ListItemData();
                    data.name = dc.name;
                    data.eventId = eventId;
                    data.numInstances = numInstances;
                    data.numTris = numTris;


                    ListViewItem listItem = new ListViewItem(data.eventId.ToString());
                    listItem.SubItems.Add(data.name);
                    listItem.SubItems.Add(data.numTris.ToString());
                    listItem.SubItems.Add(data.numInstances.ToString());
                    listItem.SubItems.Add("-");
                    listItem.SubItems.Add("-");
                    listItem.SubItems.Add("-");
                    listItem.SubItems.Add("-");
                    listItem.SubItems.Add("-");
                    listItem.SubItems.Add("-");
                    listItem.SubItems.Add("-");
                    listItem.SubItems.Add("-");
                    listItem.SubItems.Add("-");
                    listItem.SubItems.Add("Awaiting");
                    listItem.Tag = data;
                    drawcallsToExport.Items.Add(listItem);

                    DataFetcherJob job = new DataFetcherJob();
                    job.dc = dc;
                    job.listItem = listItem;
                    dataFetcher.Add(job);

                    needUpdateStatus = true;
                }
            }

            if (needUpdateStatus)
            {
                UpdateStatus();
            }
        }

        private void addDrawCallsButton_Click(object sender, EventArgs e)
        {
            AddDrawCalls(selectedEventFrom, selectedEventTo);
        }


        DataFetcherResult[] GetSelectedDrawCalls()
        {
            List<DataFetcherResult> result = new List<DataFetcherResult>();
            foreach (ListViewItem v in drawcallsToExport.SelectedItems)
            {
                ListItemData data = v.Tag as ListItemData;
                if (data == null)
                {
                    return null;
                }
                if (data.fetchResult == null)
                {
                    return null;
                }
                result.Add(data.fetchResult);
            }
            return result.ToArray();
        }

        private void removeSelected_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem v in drawcallsToExport.SelectedItems)
            {
                ListItemData data = v.Tag as ListItemData;
                selectedDrawcalls.Remove(data.eventId);
                drawcallsToExport.Items.Remove(v);
            }

            UpdateStatus();
        }

        private void clearDrawCalls_Click(object sender, EventArgs e)
        {
            drawcallsToExport.Items.Clear();
            selectedDrawcalls.Clear();
            removeSelected.Enabled = false;
            synchronize.Enabled = false;
            synchronize.Text = "Select EID";
            UpdateStatus();
        }

        private void drawcallsToExport_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            removeSelected.Enabled = true;
            ListItemData data = e.Item.Tag as ListItemData;
            if (data != null)
            {
                synchronize.Enabled = true;
                synchronize.Text = "Select EID " + data.eventId.ToString();
            }
        }

        private void synchronize_Click(object sender, EventArgs e)
        {
            if (drawcallsToExport.SelectedItems.Count <= 0)
                return;

            ListItemData data = drawcallsToExport.SelectedItems[0].Tag as ListItemData;
            m_Core.SetEventID(this, data.eventId);
        }

        private void ExporterView_VisibleChanged(object sender, EventArgs e)
        {
            bool isVisible = this.Visible;
            dataFetcher.SetPause(!isVisible);
        }

        private void drawcallsToExport_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (drawCallsSorter.GetColumn() != e.Column)
            {
                drawCallsSorter.SetSortOrder(SortOrder.Ascending);
                drawCallsSorter.SetColumn(e.Column);
            } else
            {
                // invert sort order
                SortOrder order = drawCallsSorter.GetSortOrder();
                if (order == SortOrder.Ascending)
                    order = SortOrder.Descending;
                else
                    order = SortOrder.Ascending;

                drawCallsSorter.SetSortOrder(order);
            }
           
            drawcallsToExport.Sort();
        }


        private void UpdateExportProgress(float v)
        {
            this.BeginInvoke(new Action(() =>
            {
                if (modalExportProgress != null)
                {
                    modalExportProgress.LogfileProgressBegin();
                    modalExportProgress.LogfileProgress(v);
                }
            }));
        }

        private void UpdateExportMessage(string msg)
        {
            this.BeginInvoke(new Action(() =>
            {
                if (modalExportProgress != null)
                {
                    modalExportProgress.SetModalText(msg);
                }
            }));
        }

        private bool DoExport(string fbxFileName, FbxExportOptions exportOptions, out int meshCount, out int texturesCount)
        {
            meshCount = 0;
            texturesCount = 0;

            if (exportOptions.inputData == null)
            {
                return false;
            }


            int inputMeshesCount = exportOptions.inputData.Length;
            if (inputMeshesCount == 0)
            {
                return false;
            }

            bool isRightHanded = false;
            if (m_Core.CurGLPipelineState != null || m_Core.CurGLPipelineState != null)
            {
                isRightHanded = true;
            }

            bool hasWorldOrigin = false;
            Mat4x4 invWorldOrigin = new Mat4x4();
            Mat4x4 worldOrigin = new Mat4x4();

            AnalysisResult[] origins = FindBestWorldOrigins(exportOptions.inputData);


            int materialTextureIndex = exportOptions.textureSlotIndex;
            Dictionary<ResourceId, TextureDescriptor> usedSceneTextures = new Dictionary<ResourceId, TextureDescriptor>();

            List<Tuple<long, long>> fbxConnections = new List<Tuple<long, long>>();
            List<Tuple<long, long, string>> fbxNamedConnections = new List<Tuple<long, long, string>>();
            StringBuilder sb = new StringBuilder(1024 * 1024 * 16);
            FbxCreateHeader(fbxFileName, sb);
            FbxBeginObjects(sb);

            long frustumsGroupId = 0;
            long frustumsMaterialId = 0;
            if (exportOptions.exportFrustums)
            {
                // Create 'Frustums' group
                frustumsGroupId = FbxGenerateId();
                FbxCreateGroup("Frustums", frustumsGroupId, sb);
                // link group to the scene root
                fbxConnections.Add(new Tuple<long, long>(frustumsGroupId, 0));

                frustumsMaterialId = FbxMaterial("MtlFrustum", 0.0f, 0.18f, 1.0f, 0.8f, sb);
            }

            int numMeshes = 0;
            int originsCount = 0;
            if (origins != null)
            {
                originsCount = origins.Length;
            }

            for (int originIndex = 0; originIndex < originsCount; originIndex++)
            {
                AnalysisResult origin = origins[originIndex];
                if (origin == null)
                {
                    continue;
                }

                long groupId = FbxGenerateId();

                hasWorldOrigin = true;
                invWorldOrigin = origin.invWorldViewProjection;
                worldOrigin = origin.worldViewProjection;

                for (int i = 0; i < exportOptions.inputData.Length; i++)
                {
                    if (exportOptions.inputData[i].analysisResults == null)
                    {
                        continue;
                    }

                    //Skip draw call if don't write to color
                    if (exportOptions.filter == FbxExportFilter.SKIP_DEPTH_ONLY && !exportOptions.inputData[i].isExportColor)
                    {
                        continue;
                    }

                    //Skip draw call if don't write to color or write to depth
                    if (exportOptions.filter == FbxExportFilter.INCLUDE_COLOR_ONLY && (!exportOptions.inputData[i].isExportColor || exportOptions.inputData[i].isExportDepth))
                    {
                        continue;
                    }

                    //Skip draw call if don't write to depth or write to color
                    if (exportOptions.filter == FbxExportFilter.INCLUDE_DEPTH_ONLY && (exportOptions.inputData[i].isExportColor || !exportOptions.inputData[i].isExportDepth))
                    {
                        continue;
                    }

                    if (exportOptions.inputData[i].analysisResults[0].projectionBucket == originIndex)
                    {
                        long textureId = 0;
                        if (exportOptions.inputData[i].textures.Length > materialTextureIndex)
                        {
                            TextureDescriptor texture = exportOptions.inputData[i].textures[materialTextureIndex];
                            TextureDescriptor t = null;
                            if (!usedSceneTextures.TryGetValue(texture.id, out t))
                            {
                                usedSceneTextures.Add(texture.id, texture);
                            }
                            else
                            {
                                texture = t;
                            }
                            textureId = texture.fbxId;
                        }

                        long meshMaterialId = FbxMaterial("Mtl_" + exportOptions.inputData[i].eventId.ToString(), 1.0f, 1.0f, 1.0f, 1.0f, sb);

                        if (textureId != 0)
                        {
                            fbxNamedConnections.Add(new Tuple<long, long, string>(textureId, meshMaterialId, "DiffuseColor"));
                        }


                        bool hasWorldMatrix = false;
                        bool disableTransformCalc = !exportOptions.exportWorldTransforms;

                        Tuple<long, long> fbxObject = FbxCreateMesh(exportOptions.inputData[i], hasWorldOrigin, disableTransformCalc, invWorldOrigin, worldOrigin, originIndex, !isRightHanded, exportOptions.exportVerticalSwapUV, sb, out hasWorldMatrix, exportOptions);

                        // link mesh and transform
                        fbxConnections.Add(fbxObject);

                        // link to group
                        fbxConnections.Add(new Tuple<long, long>(fbxObject.Item2, groupId));

                        // link material to mesh
                        fbxConnections.Add(new Tuple<long, long>(meshMaterialId, fbxObject.Item2));

                        numMeshes++;

                        float progress = (float)numMeshes / (float)inputMeshesCount;
                        UpdateExportProgress(progress);
                    }
                }


                if (hasWorldOrigin && numMeshes > 0)
                {
                    FbxCreateGroup("Proj_" + originIndex.ToString(), groupId, sb);
                    // link group to the scene root
                    fbxConnections.Add(new Tuple<long, long>(groupId, 0));

                    if (exportOptions.exportFrustums)
                    {
                        Tuple<long, long> fbxFrustum = FbxCreateFrustum(invWorldOrigin, originIndex, sb);
                        // link mesh and transform
                        fbxConnections.Add(fbxFrustum);

                        // link to Frustums group
                        fbxConnections.Add(new Tuple<long, long>(fbxFrustum.Item2, frustumsGroupId));

                        // link material to frustum
                        fbxConnections.Add(new Tuple<long, long>(frustumsMaterialId, fbxFrustum.Item2));
                    }
                }
            }

            //write local drawcalls
            long localGroupId = FbxGenerateId();

            int numLocalMeshes = 0;
            for (int i = 0; i < exportOptions.inputData.Length; i++)
            {
                //Skip draw call if don't write to color
                if (exportOptions.filter == FbxExportFilter.SKIP_DEPTH_ONLY && !exportOptions.inputData[i].isExportColor)
                {
                    continue;
                }

                //Skip draw call if don't write to color or write to depth
                if (exportOptions.filter == FbxExportFilter.INCLUDE_COLOR_ONLY && (!exportOptions.inputData[i].isExportColor || exportOptions.inputData[i].isExportDepth))
                {
                    continue;
                }

                //Skip draw call if don't write to depth or write to color
                if (exportOptions.filter == FbxExportFilter.INCLUDE_DEPTH_ONLY && (exportOptions.inputData[i].isExportColor || !exportOptions.inputData[i].isExportDepth))
                {
                    continue;
                }


                if (exportOptions.inputData[i].analysisResults == null ||
                    origins == null ||
                    exportOptions.inputData[i].analysisResults[0].projectionBucket < 0 ||
                    origins[exportOptions.inputData[i].analysisResults[0].projectionBucket] == null)
                {

                    long textureId = 0;
                    if (exportOptions.inputData[i].textures.Length > materialTextureIndex)
                    {
                        TextureDescriptor texture = exportOptions.inputData[i].textures[materialTextureIndex];
                        TextureDescriptor t = null;
                        if (!usedSceneTextures.TryGetValue(texture.id, out t))
                        {
                            usedSceneTextures.Add(texture.id, texture);
                        }
                        else
                        {
                            texture = t;
                        }
                        textureId = texture.fbxId;
                    }

                    long meshMaterialId = FbxMaterial("Mtl_" + exportOptions.inputData[i].eventId.ToString(), 1.0f, 1.0f, 1.0f, 1.0f, sb);

                    if (textureId != 0)
                    {
                        fbxNamedConnections.Add(new Tuple<long, long, string>(textureId, meshMaterialId, "DiffuseColor"));
                    }

                    bool hasWorldMatrix = false;

                    Mat4x4 identity = Mat4x4.Identity();

                    Tuple<long, long> fbxObject = FbxCreateMesh(exportOptions.inputData[i], false, false, identity, identity, 0, isRightHanded, exportOptions.exportVerticalSwapUV, sb, out hasWorldMatrix, exportOptions);
                    // link mesh and transform
                    fbxConnections.Add(fbxObject);

                    // link to group
                    fbxConnections.Add(new Tuple<long, long>(fbxObject.Item2, localGroupId));

                    // link material to mesh
                    fbxConnections.Add(new Tuple<long, long>(meshMaterialId, fbxObject.Item2));

                    // link texture to material
                    //fbxConnections.Add(new Tuple<long, long>(textureId, meshMaterialId));

                    numLocalMeshes++;

                    float progress = (float)(numMeshes + numLocalMeshes) / (float)inputMeshesCount;
                    UpdateExportProgress(progress);

                }
            }

            if (numLocalMeshes > 0)
            {
                FbxCreateGroup("Local", localGroupId, sb);
                // link group to the scene root
                fbxConnections.Add(new Tuple<long, long>(localGroupId, 0));
            }

            int texIndex = 1;
            foreach (KeyValuePair<ResourceId, TextureDescriptor> texture in usedSceneTextures)
            {
                string texName = "file" + texIndex;
                string texFileName = "Tex" + texIndex + "_s" + materialTextureIndex.ToString() + ".dds";
                FbxCreateTexture(texture.Value.fbxId, texName, texFileName, sb);
                texIndex++;
            }

            FbxEndObjects(sb);
            FbxBeginConnections(sb);
            FbxBuildConnections(fbxConnections, sb);
            FbxBuildNamedConnections(fbxNamedConnections, sb);
            FbxEndConnections(sb);
            System.IO.File.WriteAllText(fbxFileName, sb.ToString());

            if (exportOptions.exportTextures)
            {
                UpdateExportMessage("Export textures...");
                UpdateExportProgress(0.0f);

                int usedTexturesCount = usedSceneTextures.Count;

                string textureDirName = Path.GetDirectoryName(Path.GetFullPath(fbxFileName));

                texIndex = 1;
                TextureSave saveData = new TextureSave();
                foreach (KeyValuePair<ResourceId, TextureDescriptor> texture in usedSceneTextures)
                {
                    saveData.id = texture.Key;
                    saveData.typeHint = texture.Value.typeHint;

                    string texFileName = "Tex" + texIndex + "_s" + materialTextureIndex.ToString() + ".dds";
                    string texName = string.Format("{0}\\{1}", textureDirName, texFileName);
                    bool ret = false;
                    m_Core.Renderer.Invoke((ReplayRenderer r) =>
                    {
                        ret = r.SaveTexture(saveData, texName);
                    });

                    float progress = (float)(texIndex - 1) / (float)usedTexturesCount;
                    UpdateExportProgress(progress);

                    texIndex++;
                }
            }

            meshCount = (numMeshes + numLocalMeshes);
            texturesCount = (texIndex-1);

            return true;
        }

        private void doExport_Click(object sender, EventArgs e)
        {
            dataFetcher.SetPause(true);

            SaveFileDialog fbxSaveDialog = new SaveFileDialog();
            fbxSaveDialog.DefaultExt = "fbx";
            fbxSaveDialog.Filter = "FBX Files (*.fbx)|*.fbx";
            fbxSaveDialog.Title = "Save selected as FBX";
            DialogResult res = fbxSaveDialog.ShowDialog();
            if (res != DialogResult.OK)
            {
                dataFetcher.SetPause(false);
                return;
            }

            string fbxFileName = fbxSaveDialog.FileName;

            FbxExportOptions exportOptions = new FbxExportOptions();
            exportOptions.inputData = GetSelectedDrawCalls();
            exportOptions.exportTextures = exportTextures.Checked;
            exportOptions.exportFrustums = exportFrustums.Checked;
            exportOptions.exportWorldTransforms = exportWorldXForms.Checked;
            exportOptions.exportVerticalSwapUV = exportSwapUV.Checked;
            exportOptions.uvIndex = uvChannelToExport.SelectedIndex;
            exportOptions.textureSlotIndex = texSlotToExport.SelectedIndex;
            exportOptions.filter = (FbxExportFilter)typeToExport.SelectedIndex;

            int meshCount = 0;
            int texturesCount = 0;

            modalExportProgress = null;
            bool exportResults = false;

            Thread th = Helpers.NewThread(new ThreadStart(() =>
            {
                exportResults = DoExport(fbxFileName, exportOptions, out meshCount, out texturesCount);
            }));

            th.Start();

            // wait some time before popping up a progress bar
            th.Join(500);

            if (th.IsAlive)
            {
                modalExportProgress = new ProgressPopup((ModalCloseCallback)delegate
                {
                    return !th.IsAlive;
                }, true);
                modalExportProgress.SetModalText("Export FBX...");

                //Show modal dialog
                modalExportProgress.ShowDialog();

                modalExportProgress = null;
            }

            if (exportResults == false)
            {
                MessageBox.Show("Draw call processing still not finished yet.\nPlease wait until drawcalls selected to export was processed.");
            } else
            {
                MessageBox.Show(string.Format("Export Successful!\n\nMesh count {0}\nTextures count {1}", meshCount, texturesCount));
            }

            dataFetcher.SetPause(false);
        }


        private int CalculateBestWorldOriginScore(Mat4x4 worldViewProjection, Mat4x4 invWorldViewProjection, List<AnalysisResult> analysisResults, int fromIndex, int maxCount)
        {
            double scaleThreshold = 0.001;
            int score = 0;
            int checkCount = 0;
            for (int i = fromIndex; i < analysisResults.Count; i++)
            {
                AnalysisResult analysisResult = analysisResults[i];

                Mat4x4 world = analysisResult.invWorldViewProjection * worldViewProjection;

                // world matrix does not contain rotation component
                if (world.NumberOfZeroesInsideRotationComponent() == 6)
                {
                    score += 2;
                }

                double m00 = Math.Abs(world._00);
                double m01 = Math.Abs(world._01);
                double m02 = Math.Abs(world._02);
                double m10 = Math.Abs(world._10);
                double m11 = Math.Abs(world._11);
                double m12 = Math.Abs(world._12);
                double m20 = Math.Abs(world._20);
                double m21 = Math.Abs(world._21);
                double m22 = Math.Abs(world._22);

                double s0 = m00 * m00 + m01 * m01 + m02 * m02;
                double s1 = m10 * m10 + m11 * m11 + m12 * m12;
                double s2 = m20 * m20 + m21 * m21 + m22 * m22;

                // resulting world matrix does not contain non-uniform scale
                if (Math.Abs(s0 - s1) < scaleThreshold && Math.Abs(s1 - s2) < scaleThreshold && Math.Abs(s2 - s0) < scaleThreshold)
                {
                    score += 30;
                } else
                {
                    //one of the matrix (analysisResult.worldViewProjection or worldViewProjection)
                    //contains non-uniform scale
                    score -= 10;
                }

                // 
                checkCount++;
                if (checkCount >= maxCount)
                {
                    return score;
                }
            }
            return score;
        }

        private static int FindProjectionBase(List<AnalysisResult> linearizedAnalysisResults, int startIndex)
        {
            for (int i = startIndex; i < linearizedAnalysisResults.Count; i++)
            {
                if (linearizedAnalysisResults[i].projectionBucket == -1)
                {
                    return i;
                }
            }
            return -1;
        }

        private AnalysisResult[] FindBestWorldOrigins(DataFetcherResult[] data)
        {
            // Linearize valid AnalysisResults
            List<AnalysisResult> analysisResults = new List<AnalysisResult>(data.Length * 2);
            List<AnalysisResult> analysisResultsNoWVP = new List<AnalysisResult>(data.Length * 2);
            for (int i = 0; i < data.Length; i++)
            {
                DataFetcherResult fetchedResults = data[i];
                if (fetchedResults.analysisResults == null)
                {
                    continue;
                }

                for(UInt32 instanceId = 0; instanceId < fetchedResults.numInstances; instanceId++)
                {
                    AnalysisResult analysisResult = fetchedResults.analysisResults[instanceId];
                    if (analysisResult.hasWorldViewProjection == false)
                    {
                        analysisResult.projectionBucket = -1;
                        analysisResultsNoWVP.Add(analysisResult);
                    } else
                    {
                        analysisResult.projectionBucket = -1;
                        analysisResults.Add(analysisResult);
                    }
                }
            }

            // calculate projection buckets (drawcalls with different projection matrices)
            List<AnalysisResult> projectionBuckets = new List<AnalysisResult>(16);

            double pEps = 0.05;
            int projectionBaseIndex = 0;


            // For drawcalls with reconstructed WVP
            //
            for (int projectionBucketIndex = 0; ; projectionBucketIndex++)
            {
                //find new projection base
                projectionBaseIndex = FindProjectionBase(analysisResults, projectionBaseIndex);
                if (projectionBaseIndex < 0)
                {
                    break;
                }

                // Find all draw calls with same projection matrix
                AnalysisResult projectionBase = analysisResults[projectionBaseIndex];
                projectionBuckets.Add(projectionBase);
                for (int i = projectionBaseIndex; i < analysisResults.Count; i++)
                {
                    if (analysisResults[i].projectionBucket >= 0)
                    {
                        continue;
                    }

                    Mat4x4 world = analysisResults[i].invWorldViewProjection * projectionBase.worldViewProjection;
                    double d0 = Math.Abs(world._30);
                    double d1 = Math.Abs(world._31);
                    double d2 = Math.Abs(world._32);
                    double d3 = Math.Abs(world._33 - 1.0);
                    if (d0 < pEps && d1 < pEps && d2 < pEps && d3 < pEps)
                    {
                        analysisResults[i].projectionBucket = projectionBucketIndex;
                    }
                }
            }

            if (projectionBuckets.Count <= 0)
            {
                // Can't find any projection matrices!
                return null;
            }
            System.Diagnostics.Debug.Assert(projectionBuckets.Count > 0, "Can't find projection matrices!");

            // For drawcalls without reconstructed WVP
            //  try to attach to already existing projection buckets
            for (int j = 0; j < analysisResultsNoWVP.Count; j++)
            {
                AnalysisResult current = analysisResultsNoWVP[j];
                VertexEx[] vb = current.fetchedData.vertexDataInstances[current.instanceIndex];

                int bestProjectionBucketIndex = -1;
                double minError = 2.0;

                for (int i = 0; i < projectionBuckets.Count; i++)
                {
                    AnalysisResult projectionBase = projectionBuckets[i];
                    int projectionBucketIndex = projectionBase.projectionBucket;

                    double maxProjError = 0.0;
                    for (int vIndex = 0; vIndex < vb.Length; vIndex++)
                    {
                        if (vb[vIndex] == null)
                        {
                            continue;
                        }

                        VertexEx v = vb[vIndex];
                        Vec4 tmp = projectionBase.invWorldViewProjection * v.pp;
                        double vertexProjErr = Math.Abs(tmp.w - 1.0);
                        maxProjError = Math.Max(maxProjError, vertexProjErr);
                    }

                    if (maxProjError < minError)
                    {
                        minError = maxProjError;
                        bestProjectionBucketIndex = projectionBucketIndex;
                    }
                }
                current.projectionBucket = bestProjectionBucketIndex;
            }


            List<AnalysisResult> currentProjectionDrawCalls = new List<AnalysisResult>(analysisResults.Count);

            for (int i = 0; i < projectionBuckets.Count; i++)
            {
                AnalysisResult projectionBase = projectionBuckets[i];
                int currentProjectionBucket = projectionBase.projectionBucket;

                currentProjectionDrawCalls.Clear();

                for (int j = 0; j < analysisResults.Count; j++)
                {
                    if (analysisResults[j].projectionBucket == currentProjectionBucket)
                    {
                        currentProjectionDrawCalls.Add(analysisResults[j]);
                    }
                }

                AnalysisResult bestProjectionOrigin = FindBestOrigin(currentProjectionDrawCalls);
                projectionBuckets[i] = bestProjectionOrigin;
            }

            return projectionBuckets.ToArray();
        }


        private AnalysisResult FindBestOrigin(List<AnalysisResult> analysisResults)
        {
            int bestScore = 0;
            int bestIndex = -1;
            int windowSize = 80;
            for (int i = 0; i < analysisResults.Count; i++)
            {
                AnalysisResult origin = analysisResults[i];

                // Selects the matrix with the least number of zeros in rotation part.
                // Using this method we try to find a world origin matrix where the camera at least does not have a rotation along one of the axis (roll)
                int score = origin.worldViewProjection.NumberOfZeroesInsideRotationComponent() * 100000;
                score += CalculateBestWorldOriginScore(origin.worldViewProjection, origin.invWorldViewProjection, analysisResults, (i + 1), windowSize);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestIndex = i;
                }
            }
            //
            if (bestIndex < 0)
            {
                return null;
            }
            return analysisResults[bestIndex];
        }



        static void CalcInstanceNormals(DataFetcherResult drawCall, int instanceIndex, bool worldSpace, Mat4x4 invWorldOrigin, bool isLeftHanded)
        {
            VertexEx[] vb = drawCall.vertexDataInstances[instanceIndex];

            Vec3[] pos = new Vec3[vb.Length];

            // reset existing normals
            for (int i = 0; i < vb.Length; i++)
            {
                if (vb[i] == null)
                {
                    continue;
                }

                vb[i].n = new Vec3(0.0, 0.0, 0.0);

                //copy positions
                if (worldSpace)
                {
                    Vec4 tmp = invWorldOrigin * vb[i].pp;
                    double w = 1.0;
                    if (Math.Abs(tmp.w) > 0.000001)
                    {
                        w = tmp.w;
                    }

                    pos[i] = tmp.HomogeneousToCartesian();
                } else
                {
                    pos[i] = vb[i].p;
                }


            }

            // calculate
            UInt32 triCount = drawCall.numTris;
            for (UInt32 i = 0; i < triCount; i++)
            {
                UInt32 i0 = drawCall.indexBuffer[i * 3 + 0];
                UInt32 i1 = drawCall.indexBuffer[i * 3 + 1];
                UInt32 i2 = drawCall.indexBuffer[i * 3 + 2];

                if (i0 >= vb.Length || i1 >= vb.Length || i2 >= vb.Length)
                {
                    continue;
                }

                if (vb[i0] == null || vb[i1] == null || vb[i2] == null)
                {
                    continue;
                }

                Vec3 e0 = pos[i1] - pos[i0];
                Vec3 e1 = pos[i2] - pos[i0];

                Vec3 triN = Vec3.Cross(e0, e1);

                vb[i0].n = vb[i0].n + triN;
                vb[i1].n = vb[i1].n + triN;
                vb[i2].n = vb[i2].n + triN;
            }


            //normalize
            for (int i = 0; i < vb.Length; i++)
            {
                if (vb[i] == null)
                {
                    continue;
                }

                vb[i].n.Normalize();

                if (isLeftHanded)
                {
                    vb[i].n = vb[i].n * -1.0;
                }
            }

        }


        static VertexFbx[] GetFbxVertexBuffer(DataFetcherResult drawCall, bool isWorldSpace, Mat4x4 invWorldOrigin, Mat4x4 worldOrigin, bool isLeftHanded, FbxExportOptions exportOptions)
        {
            int vCount = 0;
            for (int i = 0; i < drawCall.numInstances; i++)
            {
                vCount += drawCall.vertexDataInstances[i].Length;
            }

            int gIndex = 0;
            VertexFbx[] result = new VertexFbx[vCount];

            VertexEx zeroVertex = new VertexEx();

            for (int inst = 0; inst < drawCall.numInstances; inst++)
            {
                bool needToCalculateNormals = true;
                Mat4x4 invWorld = Mat4x4.Identity();
                if (isWorldSpace && drawCall.analysisResults != null && drawCall.analysisResults[inst].hasWorldViewProjection)
                {
                    // invWorldOrigin - transform from pp space to "world" space
                    // worldOrigin - transform from "world" space to pp space
                    // invWorldViewProjection - transform from pp space to local space
                    //
                    // invWorld - transform from "world" space to local space
                    invWorld = drawCall.analysisResults[inst].invWorldViewProjection * worldOrigin;
                    if (drawCall.hasLocalNormals)
                    {
                        needToCalculateNormals = false;
                    }
                }

                if (isWorldSpace == false && drawCall.hasLocalNormals)
                {
                    needToCalculateNormals = false;
                }

                if (needToCalculateNormals)
                {
                    CalcInstanceNormals(drawCall, inst, isWorldSpace, invWorldOrigin, isLeftHanded);
                }

                VertexEx[] vb = drawCall.vertexDataInstances[inst];
                for (int i = 0; i < vb.Length; i++, gIndex++)
                {
                    VertexEx v = (vb[i] == null) ? zeroVertex : vb[i];

                    result[gIndex] = new VertexFbx();

                    if (isWorldSpace)
                    {
                        result[gIndex].p = (invWorldOrigin * v.pp).HomogeneousToCartesian();
                    } else
                    {
                        result[gIndex].p = v.p;
                    }

                    if (drawCall.analysisResults != null && isWorldSpace && drawCall.hasLocalNormals && drawCall.analysisResults[inst].hasWorldViewProjection)
                    {
                        Vec3 ntmp = v.n;
                        Vec3 ws_n = Mat4x4.MulRotation(ntmp, invWorld);
                        ws_n.Normalize();
                        result[gIndex].n = ws_n;
                    } else
                    {
                        result[gIndex].n = v.n;
                    }


                    switch(exportOptions.uvIndex)
                    {
                        case 0:
                            result[gIndex].uv = v.uv0;
                            break;
                        case 1:
                            result[gIndex].uv = v.uv1;
                            break;
                        case 2:
                            result[gIndex].uv = v.uv2;
                            break;
                        case 3:
                            result[gIndex].uv = v.uv3;
                            break;
                    }
                } // vertices iterator

            } // instance iterator

            return result;
        }

        static UInt32[] GetFbxIndexBuffer(DataFetcherResult drawCall, bool isLeftHanded)
        {
            int iCount = drawCall.indexBuffer.Length * (int)drawCall.numInstances;

            int gIndex = 0;
            UInt32[] res = new UInt32[iCount];

            UInt32 ioffset = 0;
            for (int inst = 0; inst < drawCall.numInstances; inst++)
            {
                for (int i = 0; i < drawCall.indexBuffer.Length; i += 3, gIndex += 3)
                {
                    UInt32 idx0 = drawCall.indexBuffer[i + 0] + ioffset;
                    UInt32 idx1 = drawCall.indexBuffer[i + 1] + ioffset;
                    UInt32 idx2 = drawCall.indexBuffer[i + 2] + ioffset;

                    if (isLeftHanded)
                    {
                        res[gIndex + 0] = idx0;
                        res[gIndex + 1] = idx2;
                        res[gIndex + 2] = idx1;
                    } else
                    {
                        res[gIndex + 0] = idx0;
                        res[gIndex + 1] = idx1;
                        res[gIndex + 2] = idx2;
                    }
                }
                ioffset += (UInt32)drawCall.vertexDataInstances[inst].Length;
            }

            return res;
        }


        static void FbxCreateHeader(string fileName, StringBuilder sb)
        {
            // FBX Header

            sb.AppendLine("; FBX 7.3.0 project file");
            sb.AppendLine("; Copyright (C) 1997-2010 Autodesk Inc. and/or its licensors.");
            sb.AppendLine("; All rights reserved.");
            sb.AppendLine("; ----------------------------------------------------");
            sb.AppendLine();

            sb.AppendLine("FBXHeaderExtension:  {");
            sb.AppendLine("\tFBXHeaderVersion: 1003");
            sb.AppendLine("\tFBXVersion: 7300");

            System.DateTime currentDate = System.DateTime.Now;
            sb.AppendLine("\tCreationTimeStamp:  {");
            sb.AppendLine("\t\tVersion: 1000");
            sb.AppendLine("\t\tYear: " + currentDate.Year);
            sb.AppendLine("\t\tMonth: " + currentDate.Month);
            sb.AppendLine("\t\tDay: " + currentDate.Day);
            sb.AppendLine("\t\tHour: " + currentDate.Hour);
            sb.AppendLine("\t\tMinute: " + currentDate.Minute);
            sb.AppendLine("\t\tSecond: " + currentDate.Second);
            sb.AppendLine("\t\tMillisecond: " + currentDate.Millisecond);
            sb.AppendLine("\t}");

            sb.AppendLine("\tCreator: \"RenderDoc Geometry Exporter\"");
            sb.AppendLine("\tSceneInfo: \"SceneInfo::GlobalInfo\", \"UserData\" {");
            sb.AppendLine("\t\tType: \"UserData\"");
            sb.AppendLine("\t\tVersion: 100");
            sb.AppendLine("\t\tMetaData:  {");
            sb.AppendLine("\t\t\tVersion: 100");
            sb.AppendLine("\t\t\tTitle: \"\"");
            sb.AppendLine("\t\t\tSubject: \"\"");
            sb.AppendLine("\t\t\tAuthor: \"\"");
            sb.AppendLine("\t\t\tKeywords: \"\"");
            sb.AppendLine("\t\t\tRevision: \"\"");
            sb.AppendLine("\t\t\tComment: \"\"");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t\tProperties70:  {");

            sb.AppendLine("\t\t\tP: \"DocumentUrl\", \"KString\", \"Url\", \"\", \"" + fileName + "\"");
            sb.AppendLine("\t\t\tP: \"SrcDocumentUrl\", \"KString\", \"Url\", \"\", \"" + fileName + "\"");
            sb.AppendLine("\t\t\tP: \"Original\", \"Compound\", \"\", \"\"");
            sb.AppendLine("\t\t\tP: \"Original|ApplicationVendor\", \"KString\", \"\", \"\", \"\"");
            sb.AppendLine("\t\t\tP: \"Original|ApplicationName\", \"KString\", \"\", \"\", \"\"");
            sb.AppendLine("\t\t\tP: \"Original|ApplicationVersion\", \"KString\", \"\", \"\", \"\"");
            sb.AppendLine("\t\t\tP: \"Original|DateTime_GMT\", \"DateTime\", \"\", \"\", \"\"");
            sb.AppendLine("\t\t\tP: \"Original|FileName\", \"KString\", \"\", \"\", \"\"");
            sb.AppendLine("\t\t\tP: \"LastSaved\", \"Compound\", \"\", \"\"");
            sb.AppendLine("\t\t\tP: \"LastSaved|ApplicationVendor\", \"KString\", \"\", \"\", \"\"");
            sb.AppendLine("\t\t\tP: \"LastSaved|ApplicationName\", \"KString\", \"\", \"\", \"\"");
            sb.AppendLine("\t\t\tP: \"LastSaved|ApplicationVersion\", \"KString\", \"\", \"\", \"\"");
            sb.AppendLine("\t\t\tP: \"LastSaved|DateTime_GMT\", \"DateTime\", \"\", \"\", \"\"");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");
            sb.AppendLine("}");

            sb.AppendLine("GlobalSettings:  {");
            sb.AppendLine("\tVersion: 1000");
            sb.AppendLine("\tProperties70:  {");
            sb.AppendLine("\t\tP: \"UpAxis\", \"int\", \"Integer\", \"\",1");
            sb.AppendLine("\t\tP: \"UpAxisSign\", \"int\", \"Integer\", \"\",1");
            sb.AppendLine("\t\tP: \"FrontAxis\", \"int\", \"Integer\", \"\",2");
            sb.AppendLine("\t\tP: \"FrontAxisSign\", \"int\", \"Integer\", \"\",1");
            sb.AppendLine("\t\tP: \"CoordAxis\", \"int\", \"Integer\", \"\",0");
            sb.AppendLine("\t\tP: \"CoordAxisSign\", \"int\", \"Integer\", \"\",1");
            sb.AppendLine("\t\tP: \"OriginalUpAxis\", \"int\", \"Integer\", \"\",-1");
            sb.AppendLine("\t\tP: \"OriginalUpAxisSign\", \"int\", \"Integer\", \"\",1");
            sb.AppendLine("\t\tP: \"UnitScaleFactor\", \"double\", \"Number\", \"\",1");
            sb.AppendLine("\t\tP: \"OriginalUnitScaleFactor\", \"double\", \"Number\", \"\",100");
            sb.AppendLine("\t\tP: \"AmbientColor\", \"ColorRGB\", \"Color\", \"\",0,0,0");
            sb.AppendLine("\t\tP: \"DefaultCamera\", \"KString\", \"\", \"\", \"Producer Perspective\"");
            sb.AppendLine("\t\tP: \"TimeMode\", \"enum\", \"\", \"\",11");
            sb.AppendLine("\t\tP: \"TimeSpanStart\", \"KTime\", \"Time\", \"\",0");
            sb.AppendLine("\t\tP: \"TimeSpanStop\", \"KTime\", \"Time\", \"\",479181389250");
            sb.AppendLine("\t\tP: \"CustomFrameRate\", \"double\", \"Number\", \"\",-1");
            sb.AppendLine("\t}");
            sb.AppendLine("}");

            sb.AppendLine("; Document References");
            sb.AppendLine(";------------------------------------------------------------------");
            sb.AppendLine("");
            sb.AppendLine("References:  {");
            sb.AppendLine("}");

            sb.AppendLine("; Object definitions");
            sb.AppendLine(";------------------------------------------------------------------");
            sb.AppendLine("");
            sb.AppendLine("Definitions:  {");
            sb.AppendLine("\tVersion: 100");
            sb.AppendLine("\tCount: 4");

            sb.AppendLine("\tObjectType: \"GlobalSettings\" {");
            sb.AppendLine("\t\tCount: 1");
            sb.AppendLine("\t}");

            sb.AppendLine("\tObjectType: \"Model\" {");
            sb.AppendLine("\t\tCount: 1");
            sb.AppendLine("\t\tPropertyTemplate: \"FbxNode\" {");
            sb.AppendLine("\t\t\tProperties70:  {");
            sb.AppendLine("\t\t\t\tP: \"QuaternionInterpolate\", \"enum\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"RotationOffset\", \"Vector3D\", \"Vector\", \"\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"RotationPivot\", \"Vector3D\", \"Vector\", \"\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"ScalingOffset\", \"Vector3D\", \"Vector\", \"\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"ScalingPivot\", \"Vector3D\", \"Vector\", \"\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"TranslationActive\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"TranslationMin\", \"Vector3D\", \"Vector\", \"\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"TranslationMax\", \"Vector3D\", \"Vector\", \"\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"TranslationMinX\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"TranslationMinY\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"TranslationMinZ\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"TranslationMaxX\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"TranslationMaxY\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"TranslationMaxZ\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"RotationOrder\", \"enum\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"RotationSpaceForLimitOnly\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"RotationStiffnessX\", \"double\", \"Number\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"RotationStiffnessY\", \"double\", \"Number\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"RotationStiffnessZ\", \"double\", \"Number\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"AxisLen\", \"double\", \"Number\", \"\",10");
            sb.AppendLine("\t\t\t\tP: \"PreRotation\", \"Vector3D\", \"Vector\", \"\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"PostRotation\", \"Vector3D\", \"Vector\", \"\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"RotationActive\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"RotationMin\", \"Vector3D\", \"Vector\", \"\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"RotationMax\", \"Vector3D\", \"Vector\", \"\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"RotationMinX\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"RotationMinY\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"RotationMinZ\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"RotationMaxX\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"RotationMaxY\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"RotationMaxZ\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"InheritType\", \"enum\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"ScalingActive\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"ScalingMin\", \"Vector3D\", \"Vector\", \"\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"ScalingMax\", \"Vector3D\", \"Vector\", \"\",1,1,1");
            sb.AppendLine("\t\t\t\tP: \"ScalingMinX\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"ScalingMinY\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"ScalingMinZ\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"ScalingMaxX\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"ScalingMaxY\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"ScalingMaxZ\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"GeometricTranslation\", \"Vector3D\", \"Vector\", \"\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"GeometricRotation\", \"Vector3D\", \"Vector\", \"\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"GeometricScaling\", \"Vector3D\", \"Vector\", \"\",1,1,1");
            sb.AppendLine("\t\t\t\tP: \"MinDampRangeX\", \"double\", \"Number\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"MinDampRangeY\", \"double\", \"Number\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"MinDampRangeZ\", \"double\", \"Number\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"MaxDampRangeX\", \"double\", \"Number\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"MaxDampRangeY\", \"double\", \"Number\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"MaxDampRangeZ\", \"double\", \"Number\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"MinDampStrengthX\", \"double\", \"Number\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"MinDampStrengthY\", \"double\", \"Number\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"MinDampStrengthZ\", \"double\", \"Number\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"MaxDampStrengthX\", \"double\", \"Number\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"MaxDampStrengthY\", \"double\", \"Number\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"MaxDampStrengthZ\", \"double\", \"Number\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"PreferedAngleX\", \"double\", \"Number\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"PreferedAngleY\", \"double\", \"Number\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"PreferedAngleZ\", \"double\", \"Number\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"LookAtProperty\", \"object\", \"\", \"\"");
            sb.AppendLine("\t\t\t\tP: \"UpVectorProperty\", \"object\", \"\", \"\"");
            sb.AppendLine("\t\t\t\tP: \"Show\", \"bool\", \"\", \"\",1");
            sb.AppendLine("\t\t\t\tP: \"NegativePercentShapeSupport\", \"bool\", \"\", \"\",1");
            sb.AppendLine("\t\t\t\tP: \"DefaultAttributeIndex\", \"int\", \"Integer\", \"\",-1");
            sb.AppendLine("\t\t\t\tP: \"Freeze\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"LODBox\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"Lcl Translation\", \"Lcl Translation\", \"\", \"A\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"Lcl Rotation\", \"Lcl Rotation\", \"\", \"A\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"Lcl Scaling\", \"Lcl Scaling\", \"\", \"A\",1,1,1");
            sb.AppendLine("\t\t\t\tP: \"Visibility\", \"Visibility\", \"\", \"A\",1");
            sb.AppendLine("\t\t\t\tP: \"Visibility Inheritance\", \"Visibility Inheritance\", \"\", \"\",1");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");

            sb.AppendLine("\tObjectType: \"Geometry\" {");
            sb.AppendLine("\t\tCount: 1");
            sb.AppendLine("\t\tPropertyTemplate: \"FbxMesh\" {");
            sb.AppendLine("\t\t\tProperties70:  {");
            sb.AppendLine("\t\t\t\tP: \"Color\", \"ColorRGB\", \"Color\", \"\",0.8,0.8,0.8");
            sb.AppendLine("\t\t\t\tP: \"BBoxMin\", \"Vector3D\", \"Vector\", \"\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"BBoxMax\", \"Vector3D\", \"Vector\", \"\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"Primary Visibility\", \"bool\", \"\", \"\",1");
            sb.AppendLine("\t\t\t\tP: \"Casts Shadows\", \"bool\", \"\", \"\",1");
            sb.AppendLine("\t\t\t\tP: \"Receive Shadows\", \"bool\", \"\", \"\",1");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");

            sb.AppendLine("\tObjectType: \"Material\" {");
            sb.AppendLine("\t\tCount: 1");
            sb.AppendLine("\t\tPropertyTemplate: \"FbxSurfaceLambert\" {");
            sb.AppendLine("\t\t\tProperties70:  {");
            sb.AppendLine("\t\t\t\tP: \"ShadingModel\", \"KString\", \"\", \"\", \"Lambert\"");
            sb.AppendLine("\t\t\t\tP: \"MultiLayer\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"EmissiveColor\", \"Color\", \"\", \"A\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"EmissiveFactor\", \"Number\", \"\", \"A\",1");
            sb.AppendLine("\t\t\t\tP: \"AmbientColor\", \"Color\", \"\", \"A\",0.2,0.2,0.2");
            sb.AppendLine("\t\t\t\tP: \"AmbientFactor\", \"Number\", \"\", \"A\",1");
            sb.AppendLine("\t\t\t\tP: \"DiffuseColor\", \"Color\", \"\", \"A\",0.8,0.8,0.8");
            sb.AppendLine("\t\t\t\tP: \"DiffuseFactor\", \"Number\", \"\", \"A\",1");
            sb.AppendLine("\t\t\t\tP: \"Bump\", \"Vector3D\", \"Vector\", \"\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"NormalMap\", \"Vector3D\", \"Vector\", \"\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"BumpFactor\", \"double\", \"Number\", \"\",1");
            sb.AppendLine("\t\t\t\tP: \"TransparentColor\", \"Color\", \"\", \"A\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"TransparencyFactor\", \"Number\", \"\", \"A\",0");
            sb.AppendLine("\t\t\t\tP: \"DisplacementColor\", \"ColorRGB\", \"Color\", \"\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"DisplacementFactor\", \"double\", \"Number\", \"\",1");
            sb.AppendLine("\t\t\t\tP: \"VectorDisplacementColor\", \"ColorRGB\", \"Color\", \"\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"VectorDisplacementFactor\", \"double\", \"Number\", \"\",1");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");

            sb.AppendLine("\tObjectType: \"Texture\" {");
            sb.AppendLine("\t\tCount: 1");
            sb.AppendLine("\t\tPropertyTemplate: \"FbxFileTexture\" {");
            sb.AppendLine("\t\t\tProperties70:  {");
            sb.AppendLine("\t\t\t\tP: \"TextureTypeUse\", \"enum\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"Texture alpha\", \"Number\", \"\", \"A\",1");
            sb.AppendLine("\t\t\t\tP: \"CurrentMappingType\", \"enum\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"WrapModeU\", \"enum\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"WrapModeV\", \"enum\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"UVSwap\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"PremultiplyAlpha\", \"bool\", \"\", \"\",1");
            sb.AppendLine("\t\t\t\tP: \"Translation\", \"Vector\", \"\", \"A\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"Rotation\", \"Vector\", \"\", \"A\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"Scaling\", \"Vector\", \"\", \"A\",1,1,1");
            sb.AppendLine("\t\t\t\tP: \"TextureRotationPivot\", \"Vector3D\", \"Vector\", \"\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"TextureScalingPivot\", \"Vector3D\", \"Vector\", \"\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"CurrentTextureBlendMode\", \"enum\", \"\", \"\",1");
            sb.AppendLine("\t\t\t\tP: \"UVSet\", \"KString\", \"\", \"\", \"default\"");
            sb.AppendLine("\t\t\t\tP: \"UseMaterial\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"UseMipMap\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");

            sb.AppendLine("}");
            sb.AppendLine("");
        }

        static void FbxBeginObjects(StringBuilder sb)
        {
            sb.AppendLine("; Object properties\r\n; ------------------------------------------------------------------\r\n\r\nObjects:  {");
        }

        static void FbxEndObjects(StringBuilder sb)
        {
            sb.AppendLine("}");
        }

        static void FbxBeginConnections(StringBuilder sb)
        {
            sb.AppendLine("; Object connections\r\n;------------------------------------------------------------------\r\n\r\nConnections:  {");
        }

        static void FbxEndConnections(StringBuilder sb)
        {
            sb.AppendLine("}");
        }


        static void FbxBuildConnections(List<Tuple<long, long>> connections, StringBuilder sb)
        {
            foreach(var connection in connections)
            {
                sb.AppendFormat("\tC: \"OO\",{0},{1}", connection.Item1, connection.Item2);
                sb.AppendLine();
            }
        }

        static void FbxBuildNamedConnections(List<Tuple<long, long, string>> connections, StringBuilder sb)
        {
            foreach (var connection in connections)
            {
                sb.AppendFormat("\tC: \"OP\",{0},{1}, \"{2}\"", connection.Item1, connection.Item2, connection.Item3);
                sb.AppendLine();
            }
        }



        static long FbxGenerateId()
        {
            long id0 = System.BitConverter.ToInt64(System.Guid.NewGuid().ToByteArray(), 0);
            long id1 = System.BitConverter.ToInt64(System.Guid.NewGuid().ToByteArray(), 8);
            long id = id0 ^ id1;
            if (id < 0)
            {
                id = -id;
            }

            if (id == 0)
            {
                id = 1;
            }
            return id;
        }

        static void FbxCreateTexture(long textureId, string textureName, string fileName, StringBuilder sb)
        {
            sb.AppendLine("\tTexture: " + textureId + ", \"Texture::" + textureName + "\", \"\" {");
            sb.AppendLine("\t\tType: \"TextureVideoClip\"");
            sb.AppendLine("\t\tVersion: 202");
            sb.AppendLine("\t\tTextureName: \"Texture::" + textureName + "\"");
            sb.AppendLine("\t\tProperties70:  {");
            sb.AppendLine("\t\t\tP: \"CurrentTextureBlendMode\", \"enum\", \"\", \"\",0");
            sb.AppendLine("\t\t\tP: \"UVSet\", \"KString\", \"\", \"\",\"map1\"");
            sb.AppendLine("\t\t\tP: \"UseMaterial\", \"bool\", \"\", \"\",1");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t\tMedia: \"Video::" + textureName + "\"");
            sb.AppendLine("\t\tFileName: \"" + fileName + "\"");
            sb.AppendLine("\t\tRelativeFilename: \"" + fileName + "\"");
            sb.AppendLine("\t\tModelUVTranslation: 0,0");
            sb.AppendLine("\t\tModelUVScaling: 1,1");
            sb.AppendLine("\t\tTexture_Alpha_Source: \"None\"");
            sb.AppendLine("\t\tCropping: 0,0,0,0");
            sb.AppendLine("\t}");// Object close 

            //C: "OP",3028506339744,3028388941920, "DiffuseColor"
        }


        static long FbxMaterial(string materialName, float r, float g, float b, float a, StringBuilder sb)
        {
            float invA = 1.0f - a;

            long materialId = FbxGenerateId();
            sb.AppendLine("\tMaterial: " + materialId + ", \"Material::" + materialName + "\", \"\" {");
            sb.AppendLine("\t\tVersion: 102");
            sb.AppendLine("\t\tShadingModel: \"lambert\"");
            sb.AppendLine("\t\tMultiLayer: 0");
            sb.AppendLine("\t\tProperties70:  {");
            sb.AppendLine("\t\t\tP: \"AmbientColor\", \"Color\", \"\", \"A\",0,0,0");
            sb.AppendLine("\t\t\tP: \"DiffuseColor\", \"Color\", \"\", \"A\"," + r.ToString("F3") + "," + g.ToString("F3") + "," + b.ToString("F3"));
            sb.AppendLine("\t\t\tP: \"DiffuseFactor\", \"Number\", \"\", \"A\",1.0");
            sb.AppendLine("\t\t\tP: \"TransparentColor\", \"Color\", \"\", \"A\"," + invA.ToString("F3") + "," + invA.ToString("F3") + "," + invA.ToString("F3"));
            sb.AppendLine("\t\t\tP: \"TransparencyFactor\", \"Number\", \"\", \"A\",1");
            sb.AppendLine("\t\t\tP: \"Emissive\", \"Vector3D\", \"Vector\", \"\",0,0,0");
            sb.AppendLine("\t\t\tP: \"Ambient\", \"Vector3D\", \"Vector\", \"\",0,0,0");
            sb.AppendLine("\t\t\tP: \"Diffuse\", \"Vector3D\", \"Vector\", \"\"," + r.ToString("F3") + "," + g.ToString("F3") + "," + b.ToString("F3"));
            sb.AppendLine("\t\t\tP: \"Opacity\", \"double\", \"Number\", \"\"," + a.ToString("F3"));
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");// Object close 
            return materialId;
        }


        static Tuple<long, long> FbxCreateFrustum(Mat4x4 invWorldOrigin, int originIndex, StringBuilder sb)
        {
            uint[] ib = new uint[36];

            //
            ib[0] = 0; ib[1] = 2; ib[2] = 1;
            ib[3] = 2; ib[4] = 3; ib[5] = 1;
            //
            ib[6] = 4; ib[7] = 5; ib[8] = 6;
            ib[9] = 6; ib[10] = 5; ib[11] = 7;
            //
            ib[12] = 8; ib[13] = 9; ib[14] = 10;
            ib[15] = 10; ib[16] = 9; ib[17] = 11;
            //
            ib[18] = 12; ib[19] = 14; ib[20] = 13;
            ib[21] = 14; ib[22] = 15; ib[23] = 13;
            //
            ib[24] = 16; ib[25] = 17; ib[26] = 18;
            ib[27] = 18; ib[28] = 17; ib[29] = 19;
            //
            ib[30] = 20; ib[31] = 22; ib[32] = 21;
            ib[33] = 22; ib[34] = 23; ib[35] = 21;

            
            VertexEx[] verices = new VertexEx[24];
            for (int i = 0; i < verices.Length; i++)
            {
                verices[i] = new VertexEx();
            }

            //D3D postprojection space unit cube
            double minX = -1.0;
            double maxX =  1.0;
            double minY = -1.0;
            double maxY = 1.0;
            double minZ = 0.0;
            double maxZ = 1.0;

            verices[0].pp = new Vec4(minX, maxY, maxZ, 1.0f);
            verices[1].pp = new Vec4(minX, maxY, minZ, 1.0f);
            verices[2].pp = new Vec4(maxX, maxY, maxZ, 1.0f);
            verices[3].pp = new Vec4(maxX, maxY, minZ, 1.0f);

            verices[4].pp = new Vec4(minX, minY, maxZ, 1.0f);
            verices[5].pp = new Vec4(minX, minY, minZ, 1.0f);
            verices[6].pp = new Vec4(maxX, minY, maxZ, 1.0f);
            verices[7].pp = new Vec4(maxX, minY, minZ, 1.0f);

            verices[8].pp = new Vec4(minX, maxY, maxZ, 1.0f);
            verices[9].pp = new Vec4(minX, maxY, minZ, 1.0f);
            verices[10].pp = new Vec4(minX, minY, maxZ, 1.0f);
            verices[11].pp = new Vec4(minX, minY, minZ, 1.0f);

            verices[12].pp = new Vec4(maxX, maxY, maxZ, 1.0f);
            verices[13].pp = new Vec4(maxX, maxY, minZ, 1.0f);
            verices[14].pp = new Vec4(maxX, minY, maxZ, 1.0f);
            verices[15].pp = new Vec4(maxX, minY, minZ, 1.0f);

            verices[16].pp = new Vec4(minX, maxY, minZ, 1.0f);
            verices[17].pp = new Vec4(maxX, maxY, minZ, 1.0f);
            verices[18].pp = new Vec4(minX, minY, minZ, 1.0f);
            verices[19].pp = new Vec4(maxX, minY, minZ, 1.0f);

            verices[20].pp = new Vec4(minX, maxY, maxZ, 1.0f);
            verices[21].pp = new Vec4(maxX, maxY, maxZ, 1.0f);
            verices[22].pp = new Vec4(minX, minY, maxZ, 1.0f);
            verices[23].pp = new Vec4(maxX, minY, maxZ, 1.0f);

            //transform to "world space"
            for (int i = 0; i < verices.Length; i++)
            {
                Vec4 v4 = (invWorldOrigin * verices[i].pp);
                double den = 1.0;
                if (Math.Abs(v4.w) > 0.000001)
                {
                    den = 1.0 / v4.w;
                }

                verices[i].p = v4.AsVector3() * den;
            }

            //calculate normals
            int triCount = ib.Length / 3;
            for (int i = 0; i < triCount; i++)
            {
                uint i0 = ib[i * 3 + 0];
                uint i1 = ib[i * 3 + 1];
                uint i2 = ib[i * 3 + 2];

                Vec3 e0 = verices[i0].p - verices[i1].p;
                Vec3 e1 = verices[i1].p - verices[i2].p;
                e0.Normalize();
                e1.Normalize();

                Vec3 n = Vec3.Cross(e0, e1);

                verices[i0].n = n;
                verices[i1].n = n;
                verices[i2].n = n;
            }


            //
            // Create Geometry
            //
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            long geometryId = FbxGenerateId();
            sb.AppendLine("\tGeometry: " + geometryId + ", \"Geometry::\", \"Mesh\" {");

            // emit vertices
            sb.AppendLine("\t\tVertices: *" + (verices.Length * 3) + " {");
            sb.Append("\t\t\ta: ");
            for (int i = 0; i < verices.Length; i++)
            {
                if (i > 0)
                    sb.Append(",");

                sb.AppendFormat("{0},{1},{2}", verices[i].p.x, verices[i].p.y, verices[i].p.z);
            }
            sb.AppendLine();
            sb.AppendLine("\t\t} ");

            //emit triangles
            sb.AppendLine("\t\tPolygonVertexIndex: *" + ib.Length + " {");
            sb.Append("\t\t\ta: ");
            for (int i = 0; i < ib.Length; i += 3)
            {
                if (i > 0)
                    sb.Append(",");

                sb.AppendFormat("{0},{1},{2}", ib[i + 0], ib[i + 1], ((int)ib[i + 2] * -1) - 1);
            }
            sb.AppendLine();
            sb.AppendLine("\t\t} ");

            //emit normals
            sb.AppendLine("\t\tGeometryVersion: 124");
            sb.AppendLine("\t\tLayerElementNormal: 0 {");
            sb.AppendLine("\t\t\tVersion: 101");
            sb.AppendLine("\t\t\tName: \"\"");
            sb.AppendLine("\t\t\tMappingInformationType: \"ByPolygonVertex\"");
            sb.AppendLine("\t\t\tReferenceInformationType: \"Direct\"");

            sb.AppendLine("\t\t\tNormals: *" + (ib.Length * 3) + " {");
            sb.Append("\t\t\t\ta: ");

            for (int i = 0; i < ib.Length; i += 3)
            {
                if (i > 0)
                    sb.Append(",");

                UInt32 ib0 = ib[i + 0];
                UInt32 ib1 = ib[i + 1];
                UInt32 ib2 = ib[i + 2];

                Vec3 nrm = verices[ib0].n;
                sb.AppendFormat("{0},{1},{2},", nrm.x, nrm.y, nrm.z);

                nrm = verices[ib1].n;
                sb.AppendFormat("{0},{1},{2},", nrm.x, nrm.y, nrm.z);

                nrm = verices[ib2].n;
                sb.AppendFormat("{0},{1},{2}", nrm.x, nrm.y, nrm.z);
            }

            sb.AppendLine();
            sb.AppendLine("\t\t\t}");

            sb.AppendLine("\t\t}");

            sb.AppendLine("\t}");// Object close 


            //
            // Create Model (transform)
            //
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            long modelId = FbxGenerateId();

            string nodeName = "frustum_p" + originIndex.ToString();

            sb.AppendLine("\tModel: " + modelId + ", \"Model::" + nodeName + "\", \"Mesh\" {");
            sb.AppendLine("\t\tVersion: 232");
            sb.AppendLine("\t\tProperties70:  {");
            sb.AppendLine("\t\t\tP: \"RotationOrder\", \"enum\", \"\", \"\",4");
            sb.AppendLine("\t\t\tP: \"RotationActive\", \"bool\", \"\", \"\",1");
            sb.AppendLine("\t\t\tP: \"InheritType\", \"enum\", \"\", \"\",1");
            sb.AppendLine("\t\t\tP: \"ScalingMax\", \"Vector3D\", \"Vector\", \"\",0,0,0");
            sb.AppendLine("\t\t\tP: \"DefaultAttributeIndex\", \"int\", \"Integer\", \"\",0");

            Vec3 pos = new Vec3(0.0, 0.0, 0.0);
            Vec3 rot = new Vec3(0.0, 0.0, 0.0);
            Vec3 scl = new Vec3(1.0, 1.0, 1.0);

            sb.Append("\t\t\tP: \"Lcl Translation\", \"Lcl Translation\", \"\", \"A+\",");
            sb.AppendFormat("{0},{1},{2}", pos.x, pos.y, pos.z);
            sb.AppendLine();

            sb.AppendFormat("\t\t\tP: \"Lcl Rotation\", \"Lcl Rotation\", \"\", \"A+\",{0},{1},{2}", rot.x, rot.y, rot.z);
            sb.AppendLine();

            sb.AppendFormat("\t\t\tP: \"Lcl Scaling\", \"Lcl Scaling\", \"\", \"A\",{0},{1},{2}", scl.x, scl.y, scl.z);
            sb.AppendLine();

            sb.AppendLine("\t\t\tP: \"currentUVSet\", \"KString\", \"\", \"U\", \"map1\"");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t\tShading: T");
            sb.AppendLine("\t\tCulling: \"CullingOff\"");

            sb.AppendLine("\t}"); // Object close 

            return new Tuple<long, long>(geometryId, modelId);

        }

        static void FbxCreateGroup(string groupName, long groupId, StringBuilder sb)
        {
            sb.AppendLine("\tModel: " + groupId + ", \"Model::" + groupName + "\", \"Null\" {");
            sb.AppendLine("\t\tVersion: 232");
            sb.AppendLine("\t\tProperties70:  {");
            sb.AppendLine("\t\t\tP: \"RotationActive\", \"bool\", \"\", \"\",1");
            sb.AppendLine("\t\t\tP: \"InheritType\", \"enum\", \"\", \"\",1");
            sb.AppendLine("\t\t\tP: \"ScalingMax\", \"Vector3D\", \"Vector\", \"\",0,0,0");
            sb.AppendLine("\t\t\tP: \"DefaultAttributeIndex\", \"int\", \"Integer\", \"\",0");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t\tShading: Y");
            sb.AppendLine("\t\tCulling: \"CullingOff\"");

            sb.AppendLine("\t}"); // Object close 
        }

        static Tuple<long, long> FbxCreateMesh(DataFetcherResult drawCall, bool isWorldSpace, bool disableTransform, Mat4x4 invWorldOrigin, Mat4x4 worldOrigin, int originIndex, bool isLeftHanded, bool verticalSwapUV, StringBuilder sb, out bool hasWorldMatrix, FbxExportOptions exportOptions)
        {
            hasWorldMatrix = false;
            Vec3 t = new Vec3(0.0, 0.0, 0.0);
            Vec3 r = new Vec3(0.0, 0.0, 0.0);
            Vec3 s = new Vec3(1.0, 1.0, 1.0);
            Mat4x4 world = Mat4x4.Identity();
            
            if (drawCall.numInstances == 1 && isWorldSpace && disableTransform == false && drawCall.analysisResults != null && drawCall.analysisResults[0].hasWorldViewProjection)
            {
                // invWorldOrigin - transform from pp space to "world" space
                // worldOrigin - transform from "world" space to pp space
                // invWorldViewProjection - transform from pp space to local space
                //
                // invWorld - transform from "world" space to local space
                Mat4x4 invWorld = drawCall.analysisResults[0].invWorldViewProjection * worldOrigin;
                world = invWorld;
                if (!world.Inverse())
                {
                    world = Mat4x4.Identity();
                    hasWorldMatrix = false;
                } else
                {
                    if (!world.Decompose(out t, out r, out s))
                    {
                        world = Mat4x4.Identity();
                        hasWorldMatrix = false;
                    }
                    else
                    {
                        hasWorldMatrix = true;
                    }
                }
            }

            //
            // Create Geometry
            //
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            long geometryId = FbxGenerateId();
            sb.AppendLine("\tGeometry: " + geometryId + ", \"Geometry::\", \"Mesh\" {");

            bool worldSpaceVertexData = true;
            if (hasWorldMatrix)
            {
                worldSpaceVertexData = false;
            }

            VertexFbx[] verices = GetFbxVertexBuffer(drawCall, worldSpaceVertexData, invWorldOrigin, worldOrigin, isLeftHanded, exportOptions);

            // emit vertices
            sb.AppendLine("\t\tVertices: *" + (verices.Length * 3) + " {");
            sb.Append("\t\t\ta: ");
            for (int i = 0; i < verices.Length; i++)
            {
                if (i > 0)
                    sb.Append(",");

                sb.AppendFormat("{0},{1},{2}", verices[i].p.x, verices[i].p.y, verices[i].p.z);
            }
            sb.AppendLine();
            sb.AppendLine("\t\t} ");

            //emit triangles
            UInt32[] ib = GetFbxIndexBuffer(drawCall, isLeftHanded);

            sb.AppendLine("\t\tPolygonVertexIndex: *" + ib.Length + " {");
            sb.Append("\t\t\ta: ");
            for (int i = 0; i < ib.Length; i += 3)
            {
                if (i > 0)
                    sb.Append(",");

                sb.AppendFormat("{0},{1},{2}", ib[i + 0], ib[i + 1], ((int)ib[i + 2] * -1) - 1);
            }
            sb.AppendLine();
            sb.AppendLine("\t\t} ");

            //emit normals
            sb.AppendLine("\t\tGeometryVersion: 124");
            sb.AppendLine("\t\tLayerElementNormal: 0 {");
            sb.AppendLine("\t\t\tVersion: 101");
            sb.AppendLine("\t\t\tName: \"\"");
            sb.AppendLine("\t\t\tMappingInformationType: \"ByPolygonVertex\"");
            sb.AppendLine("\t\t\tReferenceInformationType: \"Direct\"");

            sb.AppendLine("\t\t\tNormals: *" + (ib.Length * 3) + " {");
            sb.Append("\t\t\t\ta: ");

            for (int i = 0; i < ib.Length; i += 3)
            {
                if (i > 0)
                    sb.Append(",");

                UInt32 ib0 = ib[i + 0];
                UInt32 ib1 = ib[i + 1];
                UInt32 ib2 = ib[i + 2];

                Vec3 nrm = verices[ib0].n;
                sb.AppendFormat("{0},{1},{2},", nrm.x, nrm.y, nrm.z);

                nrm = verices[ib1].n;
                sb.AppendFormat("{0},{1},{2},", nrm.x, nrm.y, nrm.z);

                nrm = verices[ib2].n;
                sb.AppendFormat("{0},{1},{2}", nrm.x, nrm.y, nrm.z);
            }

            sb.AppendLine();
            sb.AppendLine("\t\t\t}");

            sb.AppendLine("\t\t}");

            //emit uv
            sb.AppendLine("\t\tLayerElementUV: 0 {"); // the Zero here is for the first UV map
            sb.AppendLine("\t\t\tVersion: 101");
            sb.AppendLine("\t\t\tName: \"map1\"");
            sb.AppendLine("\t\t\tMappingInformationType: \"ByPolygonVertex\"");
            sb.AppendLine("\t\t\tReferenceInformationType: \"IndexToDirect\"");
            sb.AppendLine("\t\t\tUV: *" + verices.Length * 2 + " {");
            sb.Append("\t\t\t\ta: ");

            for (int i = 0; i < verices.Length; i++)
            {
                if (i > 0)
                    sb.Append(",");

                double u = verices[i].uv.x;
                double v = verices[i].uv.y;
                if (verticalSwapUV)
                {
                    v = 1.0 - v;
                }

                sb.AppendFormat("{0},{1}", u, v);
            }

            sb.AppendLine();
            sb.AppendLine("\t\t\t\t}");

            // uv indices
            sb.AppendLine("\t\t\tUVIndex: *" + ib.Length + " {");
            sb.Append("\t\t\t\ta: ");

            for (int i = 0; i < ib.Length; i += 3)
            {
                if (i > 0)
                    sb.Append(",");

                sb.AppendFormat("{0},{1},{2}", ib[i + 0], ib[i + 1], ib[i + 2]);
            }

            sb.AppendLine();
            sb.AppendLine("\t\t\t}");
            sb.AppendLine("\t\t}");

            sb.AppendLine("\t\tLayerElementMaterial: 0 {");
            sb.AppendLine("\t\t\tVersion: 101");
            sb.AppendLine("\t\t\tName: \"\"");
            sb.AppendLine("\t\t\tMappingInformationType: \"AllSame\"");
            sb.AppendLine("\t\t\tReferenceInformationType: \"IndexToDirect\"");
            sb.AppendLine("\t\t\tMaterials: *1 {");
            sb.AppendLine("\t\t\t\ta: 0");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine("\t\t}");

            sb.AppendLine("\t\tLayer: 0 {");
            sb.AppendLine("\t\t\tVersion: 101");
            sb.AppendLine("\t\t\tLayerElement:  {");
            sb.AppendLine("\t\t\t\tType: \"LayerElementNormal\"");
            sb.AppendLine("\t\t\t\tTypedIndex: 0");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine("\t\t\tLayerElement:  {");
            sb.AppendLine("\t\t\t\tType: \"LayerElementMaterial\"");
            sb.AppendLine("\t\t\t\tTypedIndex: 0");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine("\t\t\tLayerElement:  {");
            sb.AppendLine("\t\t\t\tType: \"LayerElementUV\"");
            sb.AppendLine("\t\t\t\tTypedIndex: 0");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine("\t\t}");

            sb.AppendLine("\t}");// Object close 

            //
            // Create Model (transform)
            //
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            long modelId = FbxGenerateId();

            // local space
            string nodePrefix = "lc_";
            if (hasWorldMatrix)
            {
                //world space with transform
                nodePrefix = "wt_";
            } else
            {
                if (isWorldSpace)
                {
                    //world space global
                    nodePrefix = "wg_";
                }
            }

            string nodeName = nodePrefix + drawCall.eventId.ToString() +
                "_i" + drawCall.numInstances +
                "_n" + Convert.ToInt32(drawCall.hasLocalNormals) +
                "_c" + Convert.ToInt32(drawCall.isExportColor) +
                "_m" + Convert.ToInt32(drawCall.uiHasProjMatrix) + 
                "_p" + originIndex.ToString();
            

            sb.AppendLine("\tModel: " + modelId + ", \"Model::" + nodeName  + "\", \"Mesh\" {");
            sb.AppendLine("\t\tVersion: 232");
            sb.AppendLine("\t\tProperties70:  {");


            /*
                eEULER_XYZ = 0,
                eEULER_XZY = 1,
                eEULER_YZX = 2,
                eEULER_YXZ = 3, 
                eEULER_ZXY = 4, 
                eEULER_ZYX = 5,
            */

            sb.AppendLine("\t\t\tP: \"RotationOrder\", \"enum\", \"\", \"\",0");
            sb.AppendLine("\t\t\tP: \"RotationActive\", \"bool\", \"\", \"\",1");
            sb.AppendLine("\t\t\tP: \"InheritType\", \"enum\", \"\", \"\",1");
            sb.AppendLine("\t\t\tP: \"ScalingMax\", \"Vector3D\", \"Vector\", \"\",0,0,0");
            sb.AppendLine("\t\t\tP: \"DefaultAttributeIndex\", \"int\", \"Integer\", \"\",0");

            sb.Append("\t\t\tP: \"Lcl Translation\", \"Lcl Translation\", \"\", \"A+\",");
            sb.AppendFormat("{0},{1},{2}", t.x, t.y, t.z);
            sb.AppendLine();

            sb.AppendFormat("\t\t\tP: \"Lcl Rotation\", \"Lcl Rotation\", \"\", \"A+\",{0},{1},{2}", r.x, r.y, r.z);
            sb.AppendLine();

            sb.AppendFormat("\t\t\tP: \"Lcl Scaling\", \"Lcl Scaling\", \"\", \"A\",{0},{1},{2}", s.x, s.y, s.z);
            sb.AppendLine();

            sb.AppendLine("\t\t\tP: \"currentUVSet\", \"KString\", \"\", \"U\", \"map1\"");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t\tShading: T");
            sb.AppendLine("\t\tCulling: \"CullingOff\"");

            sb.AppendLine("\t}"); // Object close 

            return new Tuple<long, long>(geometryId, modelId);
        }





    }



}
