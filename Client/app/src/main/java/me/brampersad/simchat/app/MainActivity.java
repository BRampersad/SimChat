package me.brampersad.simchat.app;

import android.app.Activity;
import android.app.ActionBar;
import android.app.Fragment;
import android.os.AsyncTask;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.view.ViewGroup;
import android.os.Build;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ListView;

import java.io.*;
import java.net.InetAddress;
import java.net.Socket;
import java.util.ArrayList;


public class MainActivity extends Activity {

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        if (savedInstanceState == null) {
            getFragmentManager().beginTransaction()
                    .add(R.id.container, new PlaceholderFragment())
                    .commit();
        }
    }


    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        // Inflate the menu; this adds items to the action bar if it is present.
        getMenuInflater().inflate(R.menu.menu_main, menu);
        return true;
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        // Handle action bar item clicks here. The action bar will
        // automatically handle clicks on the Home/Up button, so long
        // as you specify a parent activity in AndroidManifest.xml.
        int id = item.getItemId();

        //noinspection SimplifiableIfStatement
        if (id == R.id.action_settings) {
            return true;
        }

        return super.onOptionsItemSelected(item);
    }

    /**
     * A placeholder fragment containing a simple view.
     */
    public static class PlaceholderFragment extends Fragment {

        private  EditText m_MessageEditText;
        private Button m_SendButton;
        private Socket m_TCPClient;
        private AsyncTask<Socket, Void, Void> m_ReadThread;
        private ListView m_MessagesList;
        private ArrayList<String> m_Messages;
        private ArrayAdapter<String> m_MessagesAdapter;
        private final String m_IP = "192.168.1.1";
        private final int m_Port = 10000;

        public PlaceholderFragment() {
        }

        @Override
        public View onCreateView(LayoutInflater inflater, ViewGroup container,
                                 Bundle savedInstanceState) {
            View rootView = inflater.inflate(R.layout.fragment_main, container, false);

            try {

                m_TCPClient = new Socket(m_IP, m_Port);
            } catch (IOException e) {
                e.printStackTrace();
            }

            m_MessageEditText =(EditText) rootView.findViewById(R.id.messageTxt);

            m_SendButton = (Button) rootView.findViewById(R.id.sendBtn);

            m_MessagesList = (ListView) rootView.findViewById(R.id.messages);

            m_Messages = new ArrayList<String>();
            m_MessagesAdapter = new ArrayAdapter<String>(getContext(), android.R.layout.simple_spinner_item, m_Messages);
            m_MessagesList.setAdapter(m_MessagesAdapter);

            m_SendButton.setOnClickListener(new View.OnClickListener() {
                @Override
                public void onClick(View v) {
                    AsyncTask<String, Void, Void> sendTask = new AsyncTask<String, Void, Void>() {
                        @Override
                        protected Void doInBackground(String... params) {
                            synchronized (m_TCPClient) {
                                try {
                                    m_TCPClient.getOutputStream().write(params[0].getBytes());
                                    m_TCPClient.getOutputStream().flush();
                                } catch (IOException e) {
                                    e.printStackTrace();
                                }
                            }

                            return null;
                        }
                    }.execute(m_MessageEditText.getText().toString());
                }
            });

            m_ReadThread = new AsyncTask<Socket, Void, Void>() {
                @Override
                protected Void doInBackground(Socket... params) {
                    try {
                        BufferedReader reader = new BufferedReader(new InputStreamReader(params[0].getInputStream()));

                        while (true) {
                            synchronized (m_TCPClient) {

                                String message = "";
                                int charsRead = 0;
                                char[] buffer = new char[1024];

                                while ((charsRead = reader.read(buffer)) != -1) {
                                    message += new String(buffer).substring(0, charsRead);
                                }

                                final String finalMessage = message;

                                getActivity().runOnUiThread(new Runnable() {
                                    @Override
                                    public void run() {
                                        m_MessagesAdapter.add(finalMessage);
                                        m_MessagesAdapter.notifyDataSetChanged();
                                    }
                                });
                            }
                        }
                    } catch (IOException e) {
                        e.printStackTrace();
                    }


                    return null;
                }
            };

            m_ReadThread.execute(m_TCPClient);

            return rootView;
        }
    }
}
