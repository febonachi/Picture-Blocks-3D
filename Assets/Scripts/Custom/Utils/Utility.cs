using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace Utils {    
    [Serializable] public class Collider2DEvent : UnityEvent<Collider2D> { }

    public static class Utility {
        public static bool maybe => UnityEngine.Random.Range(-1, 1) == -1;

        public static float randomChance => UnityEngine.Random.Range(0f, 1f);

        public static float average(float left, float right) => (left + right) / 2f;

        public static Color transparentColor => new Color(1f, 1f, 1f, 0f);

        public static Color randomColor() => new Color(
            UnityEngine.Random.Range(0f, 1f), 
            UnityEngine.Random.Range(0f, 1f), 
            UnityEngine.Random.Range(0f, 1f)
        );

        public static Quaternion randomRotation(Vector3 axis) => Quaternion.Euler(
            axis.x * UnityEngine.Random.Range(-360f, 360f), 
            axis.y * UnityEngine.Random.Range(-360f, 360f), 
            axis.z * UnityEngine.Random.Range(-360f, 360f)
        );

        public static Vector3 randomHorizontalDirection() => new Vector3(UnityEngine.Random.Range(-1f, 1f), 0f, UnityEngine.Random.Range(-1f, 1f)).normalized;

        public static int toInt(this bool value) => value == true ? 1 :0;
        public static bool toBool(this int value) => value != 0 ? true : false;

        public static float map(this float value, float fromMin, float fromMax, float toMin, float toMax) {
            float percent = Mathf.InverseLerp(fromMin, fromMax, value);
            return Mathf.Lerp(toMin, toMax, percent);
        }

        public static void swap<T>(ref T a, ref T b){
            T tmp = a;
            a = b;
            b = tmp;
        }

        public static float angleTrigCircle(Vector3 position) {
            float angle = 90 - Mathf.Atan2(position.x, position.y) * Mathf.Rad2Deg;
            return fromEulerAngle(angle);
        }

        public static float fromEulerAngle(float angle) {
            if (angle < 0f) angle += 360f;
            return angle;
        }

        public static float signedAngle(float angle) => angle > 180f ? angle - 360 : angle;

        public static Vector3 closestPointInDirection(Transform current, Transform target, Vector3 direction) {
            Vector3 directionToTarget = current.position - target.position;
            Vector3 transformDirection = target.TransformDirection(direction).normalized;
            float pointInTransformDirection = Vector3.Dot(transformDirection, directionToTarget);
            return target.position + (transformDirection * pointInTransformDirection);
        }

        public static bool checkoutLayer(LayerMask layerMask, int layer) => (layerMask.value & (1 << layer)) != 0;

        public static float? animationClipLength(Animator animator, string clipName) {
            return animator.runtimeAnimatorController.animationClips.FirstOrDefault(clip => clip.name == clipName)?.length;
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, System.Random rng) {
            T[] elements = source.ToArray();
            for (int i = elements.Length - 1; i >= 0; i--) {
                int swapIndex = rng.Next(i + 1);
                yield return elements[swapIndex];
                elements[swapIndex] = elements[i];
            }
        }
    }

    public static class CoroutineExtension {
        private static readonly Dictionary<string, int> runners = new Dictionary<string, int>();

        #region private
        private static IEnumerator _doParallel(IEnumerator coroutine, MonoBehaviour parent, string groupName) {
            yield return parent.StartCoroutine(coroutine);
            runners[groupName]--;
        }
        #endregion

        #region public
        public static void parallel(this IEnumerator coroutine, MonoBehaviour parent, string groupName) {
            if (!runners.ContainsKey(groupName)) runners.Add(groupName, 0);

            runners[groupName]++;
            parent.StartCoroutine(_doParallel(coroutine, parent, groupName));
        }

        public static bool inProcess(string groupName) => (runners.ContainsKey(groupName) && runners[groupName] > 0);
        #endregion
    }
}
