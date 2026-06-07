using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video; // DITAMBAHKAN: Namespace untuk VideoPlayer

namespace Sample
{
    public class KidsScript : MonoBehaviour
    {
        private Animator _Animator;
        private CharacterController _Ctrl;
        private Vector3 _MoveDirection = Vector3.zero;
        private GameObject _View_Camera;
        private Transform _Light;
        private SkinnedMeshRenderer _MeshRenderer;

        // Camera control variables
        private Camera _mainCamera;
        private Vector3 _cameraOffset = new Vector3(0, 0.5f, -2f);
        private float _cameraYaw = 0f;
        private float _cameraPitch = 20f;
        private bool _isRotatingCamera = false;
        private float _cameraRotationSpeed = 2f;

        // ZOOM variables
        private float _zoomSpeed = 2f;
        private float _minZoom = -7f;
        private float _maxZoom = -1f;

        // Sprint variables (NEW)
        private bool _isSprinting = false;
        private float _normalMaxSpeed = 2f;
        private float _sprintMaxSpeed = 4f;
        private float _sprintAcceleration = 0.2f;

        // Hash
        private static readonly int IdleState = Animator.StringToHash("Base Layer.idle");
        private static readonly int MoveState = Animator.StringToHash("Base Layer.move");
        private static readonly int JumpState = Animator.StringToHash("Base Layer.jump");
        private static readonly int DamageState = Animator.StringToHash("Base Layer.damage");
        private static readonly int DownState = Animator.StringToHash("Base Layer.down");
        private static readonly int FaintState = Animator.StringToHash("Base Layer.faint");
        private static readonly int StandUpFaintState = Animator.StringToHash("Base Layer.standup_faint");

        private static readonly int JumpTag = Animator.StringToHash("Jump");
        private static readonly int DamageTag = Animator.StringToHash("Damage");
        private static readonly int FaintTag = Animator.StringToHash("Faint");

        private static readonly int SpeedParameter = Animator.StringToHash("Speed");
        private static readonly int JumpPoseParameter = Animator.StringToHash("JumpPose");

        // --- Sistem Kendali UI & Canvas (Tambahan untuk Fitur Menu & Kuis) ---
        private bool _isPlayerLocked = false;

        [Header("UI & Canvas Controls")]
        public GameObject mainMenuCanvas;
        public CanvasGroup mainMenuCanvasGroup;
        public float fadeDuration = 1.0f;
        
        [Header("Intro Video System")] // DITAMBAHKAN: Konfigurasi Video
        public VideoPlayer introVideoPlayer; // Assign komponen VideoPlayer di sini
        public GameObject videoScreenUI;     // Assign panel UI (RawImage) tempat video diputar

        void Start()
        {
            _Animator = GetComponent<Animator>();
            _Ctrl = GetComponent<CharacterController>();
            _View_Camera = GameObject.Find("Main Camera");
            _mainCamera = _View_Camera.GetComponent<Camera>();
            _Light = GameObject.Find("Directional Light").transform;

            _MeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            if (_MeshRenderer == null)
            {
                Debug.LogError("SkinnedMeshRenderer tidak ditemukan pada " + gameObject.name);
            }
            
            // --- Logika Awal Jalankan Menu ---
            if (mainMenuCanvas != null) mainMenuCanvas.SetActive(true);
            if (mainMenuCanvasGroup != null) mainMenuCanvasGroup.alpha = 1f;
            
            // Pastikan layar video mati di awal
            if (videoScreenUI != null) videoScreenUI.SetActive(false);
            
            // Aktifkan kursor mouse untuk klik menu
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            
            // Kunci pergerakan si anak
            LockPlayer(true);
        }

        void Update()
        {
           // Jika pemain dikunci, matikan pembacaan mekanik pergerakan dan kamera
            if (_isPlayerLocked)
            {
                // Tetap jalankan gravitasi agar karakter tidak melayang jatuh bebas saat menu aktif
                GRAVITY(); 
                return; 
            }
            STATUS();
            GRAVITY(); 
            CAMERA();
            DIRECTION_LIGHT();
            if (!_Status.ContainsValue(true))
            {
                MOVE();
                JUMP();
                DAMAGE();
                FAINT();
            }
            if (_Status.ContainsValue(true))
            {
                int status_name = 0;
                foreach (var i in _Status)
                {
                    if (i.Value == true)
                    {
                        status_name = i.Key;
                        break;
                    }
                }
                if (status_name == Jump)
                {
                    MOVE();
                    JUMP();
                    FAINT();
                }
                else if (status_name == Damage)
                {
                    DAMAGE();
                }
                else if (status_name == Faint)
                {
                    FAINT();
                }
            }
        }

        //--------------------------------------------------------------------- STATUS
        private const int Jump = 1;
        private const int Damage = 2;
        private const int Faint = 3;
        private Dictionary<int, bool> _Status = new Dictionary<int, bool>
        {
            {Jump, false },
            {Damage, false },
            {Faint, false },
        };

        private void STATUS()
        {
            if (_Animator.GetCurrentAnimatorStateInfo(0).tagHash == JumpTag)
                _Status[Jump] = true;
            else
                _Status[Jump] = false;

            if (_Animator.GetCurrentAnimatorStateInfo(0).tagHash == DamageTag)
                _Status[Damage] = true;
            else
                _Status[Damage] = false;

            if (_Animator.GetCurrentAnimatorStateInfo(0).tagHash == FaintTag)
                _Status[Faint] = true;
            else
                _Status[Faint] = false;
        }

        //--------------------------------------------------------------------- CAMERA (No auto-center + Zoom)
        private void CAMERA()
        {
            if (_mainCamera == null) return;

            if (Input.GetMouseButtonDown(1))
                _isRotatingCamera = true;
            if (Input.GetMouseButtonUp(1))
                _isRotatingCamera = false;

            if (_isRotatingCamera)
            {
                float mouseX = Input.GetAxis("Mouse X") * _cameraRotationSpeed;
                float mouseY = Input.GetAxis("Mouse Y") * _cameraRotationSpeed;

                _cameraYaw += mouseX;
                _cameraPitch -= mouseY;
                _cameraPitch = Mathf.Clamp(_cameraPitch, -30f, 80f);
            }

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                _cameraOffset.z += scroll * _zoomSpeed;
                _cameraOffset.z = Mathf.Clamp(_cameraOffset.z, _minZoom, _maxZoom);
            }

            Quaternion camRotation = Quaternion.Euler(_cameraPitch, _cameraYaw, 0f);
            Vector3 worldOffset = camRotation * _cameraOffset;
            _mainCamera.transform.position = transform.position + worldOffset;
            _mainCamera.transform.LookAt(transform.position + Vector3.up * 0.5f);
        }

        //--------------------------------------------------------------------- DIRECTION_LIGHT
        private void DIRECTION_LIGHT()
        {
            if (_Light == null || _MeshRenderer == null) return;
            Vector3 pos = _Light.position - transform.position;
            _MeshRenderer.material.SetVector("_LightDir", pos);
        }

        //--------------------------------------------------------------------- GRAVITY
        private void GRAVITY()
        {
            if (CheckGrounded())
            {
                if (_MoveDirection.y < -0.1f)
                    _MoveDirection.y = -0.1f;
            }
            _MoveDirection.y -= 0.1f;
            _Ctrl.Move(_MoveDirection * Time.deltaTime);
        }

        //--------------------------------------------------------------------- isGrounded
        private bool CheckGrounded()
        {
            if (_Ctrl.isGrounded) return true;
            Ray ray = new Ray(this.transform.position + Vector3.up * 0.1f, Vector3.down);
            float range = 0.11f;
            return Physics.Raycast(ray, range);
        }

        //--------------------------------------------------------------------- MOVE (WASD relative to camera + sprint + air control)
        private void MOVE()
        {
            // --- Sprint detection (NEW) ---
            _isSprinting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            // --- Speed management ---
            float currentSpeed = _Animator.GetFloat(SpeedParameter);
            float targetSpeed = currentSpeed;

            if (_isSprinting && (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)))
            {
                // Sprinting: increase speed towards sprintMaxSpeed
                targetSpeed = Mathf.MoveTowards(currentSpeed, _sprintMaxSpeed, _sprintAcceleration);
            }
            else
            {
                // Normal speed management with Z key
                if (Input.GetKey(KeyCode.Z))
                {
                    targetSpeed = Mathf.MoveTowards(currentSpeed, _normalMaxSpeed, 0.05f);
                }
                else
                {
                    targetSpeed = Mathf.MoveTowards(currentSpeed, 1f, 0.03f);
                }
            }
            // Clamp speed between 1 and sprintMaxSpeed
            targetSpeed = Mathf.Clamp(targetSpeed, 1f, _sprintMaxSpeed);
            _Animator.SetFloat(SpeedParameter, targetSpeed);

            // --- Get camera directions ---
            Vector3 camForward = _mainCamera.transform.forward;
            Vector3 camRight = _mainCamera.transform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            // --- Read input ---
            float horizontal = 0f;
            float vertical = 0f;
            if (Input.GetKey(KeyCode.W)) vertical = 1f;
            if (Input.GetKey(KeyCode.S)) vertical = -1f;
            if (Input.GetKey(KeyCode.D)) horizontal = 1f;
            if (Input.GetKey(KeyCode.A)) horizontal = -1f;

            Vector3 moveDirection = (camForward * vertical + camRight * horizontal).normalized;
            Vector3 velocity = moveDirection * targetSpeed;
            bool isMoving = moveDirection.magnitude > 0.1f;

            // --- Rotate character (also allowed in air for air control) ---
            if (isMoving && _Animator.GetCurrentAnimatorStateInfo(0).tagHash != DamageTag && _Animator.GetCurrentAnimatorStateInfo(0).tagHash != FaintTag)
            {
                Quaternion targetRot = Quaternion.LookRotation(velocity);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
            }

            // --- Apply movement (air control: always allowed if not damaged/fainted) ---
            // Condition: not in damage or faint state, and not in transition? We simply check tag.
            if (isMoving && _Animator.GetCurrentAnimatorStateInfo(0).tagHash != DamageTag && _Animator.GetCurrentAnimatorStateInfo(0).tagHash != FaintTag)
            {
                _MoveDirection = new Vector3(velocity.x, _MoveDirection.y, velocity.z);
                _Ctrl.Move(_MoveDirection * Time.deltaTime);
                _MoveDirection.x = 0;
                _MoveDirection.z = 0;

                // Transition to MoveState if not already (only when grounded? but we allow airborne move animation)
                if (_Animator.GetCurrentAnimatorStateInfo(0).fullPathHash != MoveState &&
                    !_Animator.IsInTransition(0) &&
                    _Animator.GetCurrentAnimatorStateInfo(0).tagHash != JumpTag) // Don't override jump anim
                {
                    _Animator.CrossFade(MoveState, 0.1f);
                }
            }
            else if (!isMoving && _Animator.GetCurrentAnimatorStateInfo(0).tagHash != JumpTag &&
                     _Animator.GetCurrentAnimatorStateInfo(0).tagHash != DamageTag &&
                     _Animator.GetCurrentAnimatorStateInfo(0).tagHash != FaintTag &&
                     !_Animator.IsInTransition(0))
            {
                _Animator.CrossFade(IdleState, 0.1f);
            }
        }

        //--------------------------------------------------------------------- JUMP (Space) with lower jump height
        private void JUMP()
        {
            if (CheckGrounded())
            {
                if (Input.GetKeyDown(KeyCode.Space)
                    && _Animator.GetCurrentAnimatorStateInfo(0).tagHash != JumpTag
                    && !_Animator.IsInTransition(0))
                {
                    _Animator.CrossFade(JumpState, 0.1f, 0, 0);
                    _MoveDirection.y = 2f;   // NEW: reduced from 8 to 6.5
                    _Animator.SetFloat(JumpPoseParameter, _MoveDirection.y);
                }
                if (_Animator.GetCurrentAnimatorStateInfo(0).fullPathHash == JumpState
                    && !_Animator.IsInTransition(0)
                    && JumpPoseParameter < 0)
                {
                    if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)
                        || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
                        _Animator.CrossFade(MoveState, 0.3f, 0, 0);
                    else
                        _Animator.CrossFade(IdleState, 0.3f, 0, 0);
                }
            }
            else if (!CheckGrounded())
            {
                if (_Animator.GetCurrentAnimatorStateInfo(0).fullPathHash == JumpState
                    && !_Animator.IsInTransition(0))
                    _Animator.SetFloat(JumpPoseParameter, _MoveDirection.y);

                if (_Animator.GetCurrentAnimatorStateInfo(0).fullPathHash != JumpState
                    && !_Animator.IsInTransition(0))
                    _Animator.CrossFade(JumpState, 0.1f, 0, 0);
            }
        }

        //--------------------------------------------------------------------- DAMAGE (Q)
        private void DAMAGE()
        {
            if (Input.GetKeyDown(KeyCode.Q))
                _Animator.CrossFade(DamageState, 0.1f, 0, 0);

            if (_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1
                && _Animator.GetCurrentAnimatorStateInfo(0).tagHash == DamageTag
                && !_Animator.IsInTransition(0))
                _Animator.CrossFade(IdleState, 0.3f, 0, 0);
        }

        //--------------------------------------------------------------------- FAINT (F untuk down, E untuk stand up)
        private void FAINT()
        {
            if (Input.GetKeyDown(KeyCode.F))
                _Animator.CrossFade(DownState, 0.1f, 0, 0);

            if (_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1
                && _Animator.GetCurrentAnimatorStateInfo(0).fullPathHash == DownState
                && !_Animator.IsInTransition(0))
                _Animator.CrossFade(FaintState, 0.3f, 0, 0);

            if (Input.GetKeyDown(KeyCode.E)
                && _Animator.GetCurrentAnimatorStateInfo(0).fullPathHash == FaintState
                && !_Animator.IsInTransition(0))
                _Animator.CrossFade(StandUpFaintState, 0.1f, 0, 0);
        }
    // =====================================================================
        // --- FUNGSI TAMBAHAN UNTUK INTERAKSI CANVAS & KUIS (GHAL & TEAM) ---
        // =====================================================================

        public void LockPlayer(bool shouldLock)
        {
            _isPlayerLocked = shouldLock;

            // Jika dikunci, pastikan animasi kembali ke kondisi diam (Idle)
            if (_isPlayerLocked && _Animator != null)
            {
                _Animator.SetFloat(SpeedParameter, 1f);
                _Animator.CrossFade(IdleState, 0.1f);
            }
        }

        // FUNGSI YANG DIPERBARUI: Dipanggil oleh tombol "Play"
        public void StartGameFromMenu()
        {
            // Sembunyikan kursor
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            // Memulai proses pudar menu dan play video
            StartCoroutine(FadeOutMenuAndPlayVideoCoroutine());
        }

        // COROUTINE BARU: Pudar menu -> Putar Video -> Mulai Game
        private IEnumerator FadeOutMenuAndPlayVideoCoroutine()
        {
            // 1. Pudar Menu Utama (Fade Out)
            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                if (mainMenuCanvasGroup != null)
                {
                    mainMenuCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
                }
                yield return null;
            }
            if (mainMenuCanvas != null) mainMenuCanvas.SetActive(false);

            // 2. Cek apakah ada video player yang di-assign
            if (introVideoPlayer != null && introVideoPlayer.clip != null)
            {
                // Munculkan layar UI untuk video
                if (videoScreenUI != null) videoScreenUI.SetActive(true);
                
                // Mulai putar video
                introVideoPlayer.Play();

                // Tunggu sampai video benar-benar mulai (penting agar tidak ke-skip)
                while (!introVideoPlayer.isPlaying)
                {
                    yield return null;
                }

                // Tunggu selama video berjalan
                while (introVideoPlayer.isPlaying)
                {
                    yield return null;
                }

                // Matikan layar video setelah selesai
                if (videoScreenUI != null) videoScreenUI.SetActive(false);
            }

            // 3. Setelah menu hilang (dan video selesai, jika ada), buka kunci pemain
            LockPlayer(false);
        }   
    }
}