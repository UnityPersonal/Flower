# 🌸 Flower

[![Flower Demo](https://img.youtube.com/vi/AXhuradE5oo/0.jpg)](https://youtu.be/AXhuradE5oo?si=vojTmqkzHk5yUlCg)

---

## 📖 Project Overview [프로젝트 개요]

- 게임 **Flower** 의 모작입니다.  
- **Unity 3D** 로 제작한 게임입니다.  
- **개발 기간:** 2025.4 ~ 2025.5 (3주)

---

## ✨ Key Features [주요 기능]

### 🌱 Grass/Flower Custom Shader
- **테셀레이션** 기반으로 지형 메시에 잔디 및 꽃을 동적으로 생성  
- **GrassInteraction:** **Render Texture**, **Shader Constant Buffer**를 활용하여 잔디 바람 애니메이션 적용  

[잔디 세이더](https://github.com/UnityPersonal/Flower/blob/main/Assets/01.InGame/Shared/Shaders/MyGrass/MyGrassSSS.shader)

[꽃 세이더](https://github.com/UnityPersonal/Flower/blob/main/Assets/01.InGame/Shared/Shaders/MyGrass/MyGrassFlower.shader)

### 🌼 Spawnable Flower
- 이벤트 시스템 기반으로 다양한 연출 혼합  

[트리거 시스템 스크립트 보기](https://github.com/UnityPersonal/Flower/tree/main/Assets/01.InGame/Shared/Scripts/TriggerAnimation)

### 🎬 Timeline 연출
- 퀘스트 완료 시 Timeline 연출과 함께 다음 퀘스트 안내  

---

## 🛠 Tech Stack [기술 스택]

- **C#**  
- **Unity 2023.3.26**  
- **Custom Shader** (잔디/꽃 생성, 애니메이션)  
- **Timeline** (퀘스트 연출)  
- **GitHub** (형상 관리)  
