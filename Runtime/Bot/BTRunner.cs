using UnityEngine;

namespace Framework.Bot
{
	public class BTRunner : MonoBehaviour
	{
		private BTAgent agent;
    
		private void Awake()
		{
			agent = GetComponent<BTAgent>();
		}

		private void Update()
		{
			if (agent != null)
			{
				agent.Process();
			}
		}
	}
}
